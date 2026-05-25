using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    // HelpView displays:
    // - User Manual
    // - Frequently Asked Questions
    // - Search help topics
    // - Print manual button
    public partial class HelpView : UserControl
    {
        #region Constructor

        public HelpView()
        {
            InitializeComponent();
        }

        #endregion

        #region Notes for Groupmate

        // TODO:
        // Most of the Help screen logic should stay in HelpViewModel.
        // Avoid putting business logic here unless it is strictly UI-related.

        // Possible future UI-only tasks:
        // - Handle expand/collapse FAQ animations.
        // - Handle manual topic selection if using a list/detail layout.
        // - Handle print preview window if added later.

        #endregion
    }
}