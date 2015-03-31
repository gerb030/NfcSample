using System;

using System.Text;

using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

using Java.IO;

namespace NfcSample
{
	[Activity (Label = "NfcSample", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		public const string ViewIsolationType = "application/vnd.evision.isolationtype";

		private bool _inWriteMode;
		private NfcAdapter _nfcAdapter;
		public static readonly string Tag = "evisionNfc";

		/* fields */
		private EditText _inputText;
		private TextView _outputText;
		private TextView _alertMessage;
		private Button _button;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			_inputText = FindViewById<EditText>(Resource.Id.inputText);
			_outputText = FindViewById<TextView>(Resource.Id.outputText);
			_alertMessage = FindViewById<TextView>(Resource.Id.alertMessage);
			_button = FindViewById<Button>(Resource.Id.button);
			_button.Click += WriteTagButtonOnClick;

			// init all that cool shit
			_nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
		}

		protected override void OnNewIntent(Intent intent)
		{
			if (_inWriteMode)
			{
				_inWriteMode = false;
				var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

				if (tag == null)
				{
					return;
				}

				// These next few lines will create a payload (consisting of a string)
				// and a mimetype. NFC record are arrays of bytes. 
				var message = _inputText.Text += " " + DateTime.Now.ToString("HH:mm:ss dd/M/yyyy");
				_outputText.Text = message;
				var payload = Encoding.ASCII.GetBytes(message);
				var mimeBytes = Encoding.ASCII.GetBytes(ViewIsolationType);
				var isolationRecord = new NdefRecord(NdefRecord.TnfMimeMedia, mimeBytes, new byte[0], payload);
				var ndefMessage = new NdefMessage(new[] { isolationRecord });

				TryAndFormatTagWithMessage(tag, ndefMessage);  
				if (!TryAndWriteToTag(tag, ndefMessage))
				{
					// Maybe the write couldn't happen because the tag wasn't formatted?
					TryAndFormatTagWithMessage(tag, ndefMessage);                    
				}
			}
		}

		private bool TryAndFormatTagWithMessage(Tag tag, NdefMessage ndefMessage)
		{
			var format = NdefFormatable.Get(tag);
			if (format == null)
			{
				_alertMessage.Text = "Tag does not appear to support NDEF format.";
			}
			else
			{
				try
				{
					format.Connect();
					format.Format(ndefMessage);
					_alertMessage.Text = "Tag successfully written.";
					return true;
				}
				catch (IOException ioex)
				{
					_alertMessage.Text = "There was an error trying to format the tag: "+ioex.Message;
				}
			}
			return false;
		}

		private bool TryAndWriteToTag(Tag tag, NdefMessage ndefMessage)
		{

			// This object is used to get information about the NFC tag as 
			// well as perform operations on it.
			var ndef = Ndef.Get(tag); 
			if (ndef != null)
			{
				ndef.Connect();

				// Once written to, a tag can be marked as read-only - check for this.
				if (!ndef.IsWritable)
				{
					_alertMessage.Text = "Tag is read-only.";
				}

				// NFC tags can only store a small amount of data, this depends on the type of tag its.
				var size = ndefMessage.ToByteArray().Length;
				if (ndef.MaxSize < size)
				{
					_alertMessage.Text = "Tag doesn't have enough space.";
				}

				ndef.WriteNdefMessage(ndefMessage);
				_alertMessage.Text = "Succesfully wrote tag.";
				return true;
			}

			return false;
		}

		private void WriteTagButtonOnClick(object sender, EventArgs eventArgs)
		{
			var view = (View)sender;
			if (view.Id == Resource.Id.button)
			{
				_alertMessage.Text = "Hold the tag to the device to confirm isolation.";
				EnableWriteMode();
			}
		}

		private void EnableWriteMode()
		{
			_inWriteMode = true;

			// Create an intent filter for when an NFC tag is discovered.  When
			// the NFC tag is discovered, Android will u
			var tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
			var filters = new[] { tagDetected };

			// When an NFC tag is detected, Android will use the PendingIntent to come back to this activity.
			// The OnNewIntent method will invoked by Android.
			var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
			var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);

			if (_nfcAdapter == null) {
				var alert = new AlertDialog.Builder (this).Create ();
				alert.SetMessage ("NFC is not supported on this device.");
				alert.SetTitle ("NFC Unavailable");
				alert.SetButton ("OK", delegate {
					_button.Enabled = false;
					_alertMessage.Text = "NFC is not supported on this device.";
				});
				alert.Show ();
			} else
				_nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
		}
	}
}


