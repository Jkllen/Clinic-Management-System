using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CruzNeryClinic.ViewModels
{
    public class HelpViewModel : BaseViewModel
    {
        #region Backing Fields

        private string _manualSearchText = string.Empty;
        private string _faqSearchText = string.Empty;
        private string _selectedTab = "Manual";

        #endregion

        #region Constructor

        public HelpViewModel()
        {
            ManualTopics = new ObservableCollection<HelpManualTopic>();
            Faqs = new ObservableCollection<FaqItem>();
            FilteredManualTopics = new ObservableCollection<HelpManualTopic>();
            FilteredFaqs = new ObservableCollection<FaqItem>();

            PrintUserManualCommand = new RelayCommand(PrintUserManual);
            SelectManualTabCommand = new RelayCommand(() => SelectedTab = "Manual");
            SelectFaqTabCommand = new RelayCommand(() => SelectedTab = "FAQ");
            ToggleFaqCommand = new RelayCommand<FaqItem>(faq =>
            {
                if (faq != null) faq.IsExpanded = !faq.IsExpanded;
            });

            LoadHelpContent();
            RefreshFilteredContent();
        }

        #endregion

        #region Collections

        public ObservableCollection<HelpManualTopic> ManualTopics { get; }
        public ObservableCollection<FaqItem> Faqs { get; }
        public ObservableCollection<HelpManualTopic> FilteredManualTopics { get; }
        public ObservableCollection<FaqItem> FilteredFaqs { get; }

        #endregion

        #region Properties

        public string ManualSearchText
        {
            get => _manualSearchText;
            set
            {
                SetProperty(ref _manualSearchText, value);
                RefreshFilteredContent();
            }
        }

        public string FaqSearchText
        {
            get => _faqSearchText;
            set
            {
                SetProperty(ref _faqSearchText, value);
                RefreshFilteredContent();
            }
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        #endregion

        #region Commands

        public ICommand PrintUserManualCommand { get; }
        public ICommand SelectManualTabCommand { get; }
        public ICommand SelectFaqTabCommand { get; }
        public ICommand ToggleFaqCommand { get; }

        #endregion

        #region Load Help Content

        private void LoadHelpContent()
        {
            ManualTopics.Add(new HelpManualTopic
            {
                Title = "1. Getting Started",
                Content = "To begin using the application, launch the executable file from your desktop environment. " +
                          "Enter your unique system credentials on the login screen to verify your security access. " +
                          "Once authenticated, the system will initialize your workspace and safely direct you straight to the primary control dashboard."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "2. Dashboard",
                Content = "The main dashboard serves as your operational command center. " +
                          "It calculates and reveals dynamic counts of scheduled appointments, active walk-in waiting rooms, pending billing charts, and critical stock notifications. " +
                          "Use the interactive menu on the left side of the screen to transition fluidly between the various administrative clinic workspaces."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "3. Patient Records",
                Content = "This module allows for the total administration of all registered clinic clients. " +
                          "You can rapidly retrieve deep medical data profiles, index individual tracking numbers, update primary contact entries, and look over chronological treatment chronologies. " +
                          "Every file update is synchronized natively across the database repository."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "4. Appointment Scheduling",
                Content = "4.1 Scheduling an Appointment: To schedule a new appointment, click the 'Schedule Appointment' action button, select an existing patient profile or create a brand new entry, select your specific category of appointment type as either a Scheduled slot or a Walk-in visit, assign the precise calendar target date and time window, and type in any additional relevant descriptive clinical notes if they are required.\n\n" +
                          "4.2 Priority System: Patients holding pre-arranged scheduled appointments are automatically granted service priority over incoming unscheduled walk-in visitors. The integrated system queue processing engine constantly organizes the internal patient sequence automatically based on assigned priority status strings and recorded door arrival timestamps.\n\n" +
                          "4.3 Walk-in Patients: For incoming walk-in patients, explicitly mark the designation type as 'Walk-in' directly inside the structural appointment menu field. The underlying tracking engine immediately triggers an internal query to validate whether the matching patient records already exist within the active local database layer. If no matching profile records are detected by the query engine, you can conveniently build and insert their record identity details directly from within the open appointment creation window.\n\n" +
                          "4.4 Checking In Patients: Click the dedicated 'Check In' command button the exact moment a client arrives physically at the clinic reception desk. Executing this action command updates the record item state in real-time and automatically broadcasts a visual notice to the available clinical operations staff."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "5. Patient Billing",
                Content = "The billing ledger subsystem processes administrative checkout actions. " +
                          "It monitors treatment costs, records outstanding installment arrangements, balances invoice objects, and tallies general payment inputs. " +
                          "Finalized financial entries print directly to localized system receipt machines."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "6. Report and Analytics",
                Content = "This panel generates formal audit files covering general clinic operational velocity. " +
                          "Users can export precise fiscal summaries, total patient traffic metrics, and itemized medical supply expenditure graphs. " +
                          "Every analytical metric updates dynamically to reflect real-time operational data."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "7. Inventory Management",
                Content = "This interface monitors clinic resource consumption levels. " +
                          "It automatically tracks item counts for medical supplies, dental equipment pieces, and prescription items. " +
                          "If resource quantities dip below an established safety buffer threshold, the program signals low-stock visual flags."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "8. Backup and Recovery",
                Content = "This core system utility maintains business continuity protocols. " +
                          "Users can trigger full database archive processes to save structural records into secure backup states. " +
                          "In the event of hard disk corruption, the recovery sequence can reload previous secure historical points."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "9. Security & Data Protection",
                Content = "The system restricts module access based on active structural privileges assigned to Admin or Staff accounts. " +
                          "Detailed transaction history tables log structural edits to maintain accountability. " +
                          "Idle connection instances automatically drop after specific timing intervals to defend medical privacy."
            });

            ManualTopics.Add(new HelpManualTopic
            {
                Title = "Support & Contact",
                Content = "For technical support operations or troubleshooting inquiries regarding system behavior, please utilize the contact options listed below.\n\n" +
                          "First, contact your localized onsite system administrator to resolve fundamental login or hardware configurations.\n\n" +
                          "You can send message, acquire help, and inquire online via facebook messenger from facebook.com/cruznery.dentalclinic\n\n" +
                          "For time-critical support needs, contact the help desk network immediately by calling 0286590818"
            });

            Faqs.Add(new FaqItem
            {
                Question = "How do I add a new patient to the system?",
                Answer = "Navigate to the Patient Records workspace panel, click on the \"Add New Patient\" action trigger, input data into all mandatory profile form text boxes — including full legal name, date of birth metrics, and active contact numbers — and select save. " +
                         "The management application will automatically calculate and append a unique, non-duplicable Patient ID number to the new profile row."
            });

            Faqs.Add(new FaqItem
            {
                Question = "What is the difference between scheduled and walk-in appointments?",
                Answer = "Scheduled appointments represent pre-arranged slots reserved for a set time and date, granting them top priority placement inside the client care queue. " +
                         "Walk-in appointments represent impromptu clinic visits made without an existing reservation, which are tracked dynamically and addressed according to active doctor availability."
            });

            Faqs.Add(new FaqItem
            {
                Question = "How do I process a partial payment?",
                Answer = "Open the specific patient's billing profile, select the pending invoice, input the exact partial cash amount received, and save the transaction to update the remaining balance."
            });

            Faqs.Add(new FaqItem
            {
                Question = "How do installment payments work?",
                Answer = "Installment plans divide the total balance of expensive procedures across a set payment schedule, tracking chronological collections and identifying overdue balances."
            });

            Faqs.Add(new FaqItem
            {
                Question = "How often should I create backups?",
                Answer = "You should trigger database backup sequences at the conclusion of every business day to secure newly altered patient records and clinical financial logs."
            });

            Faqs.Add(new FaqItem
            {
                Question = "Where are backups stored?",
                Answer = "System snapshots are written to an encrypted storage folder on the local machine and automatically duplicated to a secure secondary hardware drive."
            });

            Faqs.Add(new FaqItem
            {
                Question = "What happens if I restore a backup?",
                Answer = "Executing a system restoration overwrites the active database file entirely, reverting all patient entries and ledger structures back to the exact state of that backup file."
            });

            Faqs.Add(new FaqItem
            {
                Question = "How do I know when inventory is running low?",
                Answer = "Automated system warning labels pop up directly on the main dashboard whenever an item's stock count falls beneath its configured safe operating limit."
            });

            Faqs.Add(new FaqItem
            {
                Question = "Can I edit a billing transaction after it's created?",
                Answer = "Finalized accounting records cannot be altered directly to prevent tampering; corrections require administrative credentials to issue a formal void or adjustment log."
            });

            Faqs.Add(new FaqItem
            {
                Question = "How is patient data protected?",
                Answer = "Patient information remains protected through native database encryption protocols, strict role-based access definitions, and forced station logouts during periods of inactivity."
            });

            Faqs.Add(new FaqItem
            {
                Question = "What should I do if the system is running slow?",
                Answer = "Shut down extraneous background software tools, confirm the stability of local network connections, or restart the program to clear system cache pools."
            });

            Faqs.Add(new FaqItem
            {
                Question = "How do I print receipts?",
                Answer = "Click the \"Print Receipt\" command button immediately following a successful checkout to send an optimized layout directly to your connected thermal receipt printer."
            });

            Faqs.Add(new FaqItem
            {
                Question = "Can I access the system from home?",
                Answer = "Outside system connections are entirely prohibited unless routed through an authorized virtual private network tunnel configured by the clinic's network administrator."
            });

            Faqs.Add(new FaqItem
            {
                Question = "What does the receipt number format mean?",
                Answer = "The generated receipt string combines a four-digit chronological transaction number with tracking tags that represent the active fiscal year and clinic code."
            });

            Faqs.Add(new FaqItem
            {
                Question = "How do I search for a specific patient?",
                Answer = "Type an assigned unique identification index, full last name string, or registered mobile phone number into the search filter bar located at the top of the patient dashboard view."
            });

            Faqs.Add(new FaqItem
            {
                Question = "Still have questions?",
                Answer = "If you are unable to find the answers you require within this index, please contact your designated local network system administrator or reach out to the technical IT help desk support team."
            });
        }

        #endregion

        #region Search and Filter

        private void RefreshFilteredContent()
        {
            FilteredManualTopics.Clear();
            FilteredFaqs.Clear();

            string manualQuery = _manualSearchText.Trim().ToLowerInvariant();
            string faqQuery = _faqSearchText.Trim().ToLowerInvariant();

            foreach (HelpManualTopic topic in ManualTopics)
            {
                if (string.IsNullOrEmpty(manualQuery) ||
                    topic.Title.ToLowerInvariant().Contains(manualQuery) ||
                    topic.Content.ToLowerInvariant().Contains(manualQuery))
                {
                    FilteredManualTopics.Add(topic);
                }
            }

            foreach (FaqItem faq in Faqs)
            {
                if (string.IsNullOrEmpty(faqQuery) ||
                    faq.Question.ToLowerInvariant().Contains(faqQuery) ||
                    faq.Answer.ToLowerInvariant().Contains(faqQuery))
                {
                    FilteredFaqs.Add(faq);
                }
            }
        }

        #endregion

        #region Print

        private void PrintUserManual()
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PageWidth = printDialog.PrintableAreaWidth,
                PageHeight = printDialog.PrintableAreaHeight,
                PagePadding = new Thickness(60),
                ColumnGap = 0,
                ColumnWidth = printDialog.PrintableAreaWidth
            };

            doc.Blocks.Add(new Paragraph(new Run("Cruz-Nery Dental Clinic"))
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            });

            doc.Blocks.Add(new Paragraph(new Run("User Manual and Frequently Asked Questions"))
            {
                FontSize = 14,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 24)
            });

            doc.Blocks.Add(new Paragraph(new Run("USER MANUAL"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(34, 51, 87)),
                Margin = new Thickness(0, 0, 0, 10)
            });

            foreach (HelpManualTopic topic in ManualTopics)
            {
                doc.Blocks.Add(new Paragraph(new Run(topic.Title))
                {
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 3)
                });

                doc.Blocks.Add(new Paragraph(new Run(topic.Content))
                {
                    FontSize = 11,
                    Margin = new Thickness(16, 0, 0, 4)
                });
            }

            doc.Blocks.Add(new Paragraph(new Run("FREQUENTLY ASKED QUESTIONS"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(34, 51, 87)),
                Margin = new Thickness(0, 24, 0, 10)
            });

            foreach (FaqItem faq in Faqs)
            {
                doc.Blocks.Add(new Paragraph(new Run($"Q: {faq.Question}"))
                {
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 8, 0, 3)
                });

                doc.Blocks.Add(new Paragraph(new Run($"A: {faq.Answer}"))
                {
                    FontSize = 11,
                    Margin = new Thickness(16, 0, 0, 4)
                });
            }

            IDocumentPaginatorSource paginatorSource = doc;
            printDialog.PrintDocument(paginatorSource.DocumentPaginator, "Cruz-Nery Dental Clinic - User Manual");
        }

        #endregion
    }
}
