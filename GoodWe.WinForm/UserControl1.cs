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
		items[2].SubItems[1].Text = $"{inverter.Firmware}";
		items[3].SubItems[1].Text = $"{inverter.ArmVersion}";
		items[4].SubItems[1].Text = $"{inverter.RatedPower} W";
	}

	private static string? ToMyString(object? value)
	{
		return value switch
		{
			null => null,
			double d => $"{d:F2}",
			float f => $"{f:F2}",
			DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
			_ => value.ToString() ?? "",
		};
	}

	private static void Add(ListView lv, string name, object? value)
	{
		if (value == null)
			return;
		var item = new ListViewItem(name);
		item.SubItems.Add(ToMyString(value));
		lv.Items.Add(item);
	}

	private void UpdateData(Dictionary<string, object?> data)
	{
		this.labelStatus.Text = $"Updated: {ToMyString(data["timestamp"])}";

		this.listViewData.BeginUpdate();
		this.listViewData.Items.Clear();

		Add(listViewData, "Total Power",
			$"{ToMyString(data["e_total"])} kWh");

		Add(listViewData, "Daily Power",
			$"{ToMyString(data["e_day"])} kWh");

		Add(listViewData, "Grid Code",
			$"{ToMyString(data["safety_country"])}");

		Add(listViewData, "PV1",
			$"{ToMyString(data["ipv1"])} A {ToMyString(data["vpv1"])} V");
		Add(listViewData, "PV2",
			$"{ToMyString(data["ipv2"])} A {ToMyString(data["vpv2"])} V");

		Add(listViewData, "AC Current L1/L2/L3",
			$"{ToMyString(data["igrid1"])}/{ToMyString(data["igrid2"])}/{ToMyString(data["igrid3"])} A");
		Add(listViewData, "AC Voltage L1/L2/L3", 
			$"{ToMyString(data["vgrid1"])}/{ToMyString(data["vgrid2"])}/{ToMyString(data["vgrid3"])} V");

		Add(listViewData, "AC Power",
			$"{ToMyString(data["total_inverter_power"])} W");

		Add(listViewData, "AC Frequency",
			$"{ToMyString(data["fgrid1"])} Hz");

		//foreach (var kv in data.OrderBy(k => k.Key))
		//	Add(listViewData, kv.Key, kv.Value);

		this.listViewData.EndUpdate();
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
