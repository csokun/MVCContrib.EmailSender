using System.Collections.Generic;

namespace MVCContrib.UI.ViewModels
{
    /// <summary>
    /// Base class for email models. 
    /// 
    /// Derived classes will provide specific values to substitute placeholders in concrete emails.
    /// If nothing to substitute, this class can be used directly.
    /// </summary>
    public class EmailModel 
    {
        /// <summary>
        /// Sender address.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Recipient address.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Email subject.
        /// </summary>
        public string Subject { get; set; }
    }
}
