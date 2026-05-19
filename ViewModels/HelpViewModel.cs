using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    // HelpViewModel controls the Help screen.
    // This ViewModel should contain the Help screen content and commands.
    public class HelpViewModel : BaseViewModel
    {
        #region Backing Fields

        private string searchText = string.Empty;

        #endregion

        #region Constructor

        public HelpViewModel()
        {
            ManualTopics = new ObservableCollection<HelpManualTopic>();
            Faqs = new ObservableCollection<FaqItem>();

            FilteredManualTopics = new ObservableCollection<HelpManualTopic>();
            FilteredFaqs = new ObservableCollection<FaqItem>();

            // TODO:
            // Connect this command to the Print Manual button in HelpView.xaml.
            PrintUserManualCommand = new RelayCommand(PrintUserManual);

            LoadHelpContent();
            RefreshFilteredContent();
        }

        #endregion

        #region Collections

        // Full list of manual topics.
        // TODO:
        // Add topics such as Login, Dashboard, Patients, Billing, Inventory, etc.
        public ObservableCollection<HelpManualTopic> ManualTopics { get; }

        // Full list of FAQs.
        // TODO:
        // Add common questions users may ask.
        public ObservableCollection<FaqItem> Faqs { get; }

        // Filtered list shown on the screen based on SearchText.
        public ObservableCollection<HelpManualTopic> FilteredManualTopics { get; }

        // Filtered FAQs shown on the screen based on SearchText.
        public ObservableCollection<FaqItem> FilteredFaqs { get; }

        #endregion

        #region Search Properties

        public string SearchText
        {
            get => searchText;
            set
            {
                SetProperty(ref searchText, value);

                // TODO:
                // This should filter manual topics and FAQs.
                RefreshFilteredContent();
            }
        }

        #endregion

        #region Commands

        // TODO:
        // Used by the Print Manual button.
        // This should print the user manual as hardcopy.
        public ICommand PrintUserManualCommand { get; }

        #endregion

        #region Load Help Content

        private void LoadHelpContent()
        {
            // TODO:
            // Add the actual user manual content here.
            //
            // Required manual sections:
            // 1. Login and Logout
            // 2. Forgot Password
            // 3. Dashboard
            // 4. User Management
            // 5. Patient Management
            // 6. Appointment Scheduling
            // 7. Billing and Payment
            // 8. Inventory
            // 9. Reports and Analytics
            // 10. Backup and Restore
            //
            // Example:
            // ManualTopics.Add(new HelpManualTopic
            // {
            //     Title = "Login and Logout",
            //     Content = "Step-by-step instructions here..."
            // });

            // TODO:
            // Add FAQs here.
            //
            // Required FAQ topics:
            // - Who can access User Management?
            // - How do I reset my password?
            // - What should I do before archiving a record?
            // - How do I print the user manual?
            // - What is included in Patient History?
            // - Who can access Reports and Maintenance?
            //
            // Example:
            // Faqs.Add(new FaqItem
            // {
            //     Question = "How do I reset my password?",
            //     Answer = "Use Forgot Password and answer your security questions."
            // });
        }

        #endregion

        #region Search and Filter Helpers

        private void RefreshFilteredContent()
        {
            // TODO:
            // Clear FilteredManualTopics and FilteredFaqs.
            // If SearchText is empty, copy all ManualTopics and Faqs.
            // If SearchText has text, show only matching topics/questions/answers.

            FilteredManualTopics.Clear();
            FilteredFaqs.Clear();

            // Temporary behavior:
            // Shows everything until actual filtering is implemented.
            foreach (HelpManualTopic topic in ManualTopics)
                FilteredManualTopics.Add(topic);

            foreach (FaqItem faq in Faqs)
                FilteredFaqs.Add(faq);
        }

        #endregion

        #region Print User Manual

        private void PrintUserManual()
        {
            // TODO:
            // Implement printing here.
            //
            // Expected behavior:
            // 1. Build a printable document containing:
            //    - System title
            //    - User Manual sections
            //    - FAQs
            // 2. Open PrintDialog.
            // 3. Allow staff/admin to print a hardcopy.
            //
            // Suggested WPF classes:
            // - FlowDocument
            // - Paragraph
            // - Run
            // - PrintDialog
            // - IDocumentPaginatorSource
            //
            // Later improvement:
            // Add export to PDF if required by the group.
        }

        #endregion

        #region Backend TODO Notes

        // BACKEND TODO:
        // Current Help content can start as static content in LoadHelpContent().
        // Later, this can be moved to SQLite if the group wants editable help content.
        //
        // Suggested future tables:
        //
        // HelpTopics:
        // - HelpTopicId
        // - Title
        // - Content
        // - DisplayOrder
        // - IsActive
        //
        // Faqs:
        // - FaqId
        // - Question
        // - Answer
        // - DisplayOrder
        // - IsActive
        //
        // Possible future Maintenance feature:
        // Admin can add/edit/remove FAQs and manual topics from the Maintenance module.

        #endregion
    }
}