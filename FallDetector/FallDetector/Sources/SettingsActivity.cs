using System;
using System.Linq;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Preferences;

namespace FallDetector.Sources
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {
        private Button okButton;

        private EditText phoneEditText;
        private EditText emailEditText;

        private CheckBox emailCheckBox;
        private CheckBox phoneCheckBox;
        private CheckBox notificationCheckBox;

        private ISharedPreferences prefs;
        private ISharedPreferencesEditor prefsEditor;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.SettingsLayout);

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            prefsEditor = prefs.Edit();

            Boolean checkNotification = prefs.GetBoolean(TAG.notificationTAG, false);
            Boolean checkEmail = prefs.GetBoolean(TAG.emailTAG, false);
            Boolean checkSMS = prefs.GetBoolean(TAG.smsTAG, false);

            okButton = FindViewById<Button>(Resource.Id.OkButton);

            phoneEditText = FindViewById<EditText>(Resource.Id.PhoneEditText);
            emailEditText = FindViewById<EditText>(Resource.Id.EmailEditText);

            emailCheckBox = FindViewById<CheckBox>(Resource.Id.emailCheckBox);
            phoneCheckBox = FindViewById<CheckBox>(Resource.Id.smsCheckBox);
            notificationCheckBox = FindViewById<CheckBox>(Resource.Id.notificationCheckBox);

            notificationCheckBox.Checked = checkNotification;
            emailCheckBox.Checked = checkEmail;
            phoneCheckBox.Checked = checkSMS;

            if (!emailCheckBox.Checked)
                emailEditText.Visibility = ViewStates.Invisible;

            if (!phoneCheckBox.Checked)
                phoneEditText.Visibility = ViewStates.Invisible;


            emailEditText.Click += delegate
            {
                this.onClick(emailEditText);
            };

            phoneEditText.Click += delegate
            {
                this.onClick(phoneEditText);
            };

            okButton.Click += delegate
            {
                this.onClick(okButton);
            };
            emailCheckBox.Click += delegate
            {
                this.onClick(emailCheckBox);
            };
            phoneCheckBox.Click += delegate
            {
                this.onClick(phoneCheckBox);
            };
            notificationCheckBox.Click += delegate
            {
                this.onClick(notificationCheckBox);
            };

        }

        private void onClick(View v)
        {

            if (v == okButton)
            {
                prefsEditor.PutBoolean(TAG.notificationTAG, notificationCheckBox.Checked);
                prefsEditor.PutBoolean(TAG.emailTAG, emailCheckBox.Checked);
                prefsEditor.PutBoolean(TAG.smsTAG, phoneCheckBox.Checked);
                if (emailEditText.Text != "Email")
                    prefsEditor.PutString(TAG.emailAdressTAG, emailEditText.Text);
                if (phoneEditText.Text != "Phone Number")
                    prefsEditor.PutString(TAG.phoneNumberTAG, phoneEditText.Text);

                prefsEditor.Apply();
                //this.sendEmail();

                this.Finish();
            }
            else if (v == emailCheckBox)
            {
                if (emailCheckBox.Checked)
                {
                    emailEditText.Visibility = ViewStates.Visible;
                    emailEditText.Text = "Email";
                }


                else
                    emailEditText.Visibility = ViewStates.Invisible;
            }
            else if (v == phoneCheckBox)
            {
                if (phoneCheckBox.Checked)
                {
                    phoneEditText.Visibility = ViewStates.Visible;
                    phoneEditText.Text = "Phone Number";
                }


                else
                    phoneEditText.Visibility = ViewStates.Invisible;
            }
            else if (v == emailEditText)
            {
                if (emailEditText.Text == "Email")
                    emailEditText.Text = "";
            }
            else if (v == phoneEditText)
            {
                if (phoneEditText.Text == "Phone Number")
                    phoneEditText.Text = "";
            }

        }

        public void sendEmail()
        {
            Boolean checkEmail = prefs.GetBoolean(TAG.emailTAG, false);
            String emailAdress = prefs.GetString(TAG.emailAdressTAG, null);

            if (!checkEmail || emailAdress == null)
                return;

            var emailIntent = new Intent(Android.Content.Intent.ActionSend);

            emailIntent.PutExtra(Android.Content.Intent.ExtraEmail, emailAdress);
            emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Fall detected!");
            emailIntent.PutExtra(Android.Content.Intent.ExtraText, "Fall detected!");

            emailIntent.AddFlags(ActivityFlags.NewTask);

            StartActivity(emailIntent);

        }
    }
}