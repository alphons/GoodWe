namespace GoodWe.WinForm;

public partial class UserControl1 : UserControl
{
	private Inverter? inverter;
	private CancellationTokenSource? cts;
	private System.Windows.Forms.Timer? pollTimer;

	public UserControl1()
	{
		InitializeComponent();
		cmbProtocol.SelectedIndex = 1; // UDP default
		cmbFamily.SelectedIndex = 7; // DT default
	}

	// ── Start ─────────────────────────────────────────────────────────────────

	private async void Start_Click(object sender, EventArgs e)
	{
		groupBox1.Enabled = false;
		button1.Enabled = false;
		button2.Enabled = true;

		try
		{
			bool tcp = cmbProtocol.SelectedIndex == 0;
			string host = textBox1.Text.Trim();
			FamilyEnum family = Enum.Parse< FamilyEnum>(this.cmbFamily.Text);

			cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			inverter = tcp
				? await GoodWeClient.ConnectTcpAsync(host: host, family: family, ct: cts.Token)
				: await GoodWeClient.ConnectAsync(host: host, family: family, ct: cts.Token);

			UpdateDeviceInfo();
			StartPolling();
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Connection failed:\n{ex.Message}", "Error",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
			ResetUi();
		}
	}

	// ── Stop ─────────────────────────────────────────────────────────────────

	private async void Stop_Click(object sender, EventArgs e)
	{
		StopPolling();

		if (inverter is not null)
		{
			await inverter.DisposeAsync();
			inverter = null;
		}

		ResetUi();
	}

	// ── Polling ───────────────────────────────────────────────────────────────

	private void StartPolling()
	{
		pollTimer = new System.Windows.Forms.Timer { Interval = 30000 };
		pollTimer.Tick += async (_, _) => await PollAsync();
		pollTimer.Start();

		_ = PollAsync(); // eerste keer meteen
	}

	private void StopPolling()
	{
		pollTimer?.Stop();
		pollTimer?.Dispose();
		pollTimer = null;
	}

	private async Task PollAsync()
	{
		if (inverter is null) return;
		try
		{
			var data = await inverter.ReadRuntimeDataAsync();
			Invoke(() => UpdateData(data));
		}
		catch (Exception ex)
		{
			Invoke((Delegate)(() => labelStatus.Text = $"Poll error: {ex.Message}"));
		}
	}

	// ── UI-updates ────────────────────────────────────────────────────────────

	private void UpdateDeviceInfo()
	{
		if (inverter is null)
			return;
		var items = this.listView1.Items;

		items[0].SubItems[1].Text = inverter.ModelName ?? "–";
		items[1].SubItems[1].Text = inverter.SerialNumber ?? "–";
		items[2].SubItems[1].Text = $"{inverter.RatedPower} W";
	}

	private void UpdateData(Dictionary<string, object?> data)
	{
		labelStatus.Text = $"Updated: {DateTime.Now:HH:mm:ss}";

		listViewData.BeginUpdate();
		listViewData.Items.Clear();
		foreach (var kv in data.OrderBy(k => k.Key))
		{
			if (kv.Value is null) continue;
			string display = kv.Value switch
			{
				double d => $"{d:F2}",
				float f => $"{f:F2}",
				DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
				_ => kv.Value.ToString() ?? "",
			};
			var item = new ListViewItem(kv.Key);
			item.SubItems.Add(display);
			listViewData.Items.Add(item);
		}
		listViewData.EndUpdate();
	}

	private void ResetUi()
	{
		groupBox1.Enabled = true;
		button1.Enabled = true;
		button2.Enabled = false;
		labelStatus.Text = "Stopped";
		listViewData.Items.Clear();
	}

	// ── Cleanup ───────────────────────────────────────────────────────────────

	protected override async void OnHandleDestroyed(EventArgs e)
	{
		StopPolling();
		if (inverter is not null)
		{
			await inverter.DisposeAsync();
			inverter = null;
		}
		base.OnHandleDestroyed(e);
	}
}
