
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace NfcSample
{
	[Activity, IntentFilter(new[] { "android.nfc.action.NDEF_DISCOVERED" },
		DataMimeType = MainActivity.ViewIsolationType,
		Categories = new[] { "android.intent.category.DEFAULT" })]

	public class NfcReader : Activity
	{
		/* fields */
		private TextView _outputText;
		private Button _backButton;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.IsolationViewer);
			var intentMimeType = Intent.Type ?? String.Empty;

			_outputText = FindViewById<TextView> (Resource.Id.outputText);
			_backButton = FindViewById<Button>(Resource.Id.backButton);
			_backButton.Click += (sender, args) => Finish();

			if (MainActivity.ViewIsolationType.Equals(intentMimeType)) {
				// Get the string that was written to the NFC tag, and display it.
				var rawMessages = Intent.GetParcelableArrayExtra (NfcAdapter.ExtraNdefMessages);
				var msg = (NdefMessage)rawMessages[0];
				var isolationRecord = msg.GetRecords()[0];
				var rawText = Encoding.ASCII.GetString (isolationRecord.GetPayload());
				_outputText.Text = "Isolation read: "+rawText+".";
			} else {
				_outputText.Text = "Not an eVision tag";
			}
		}
	}
}

