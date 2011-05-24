<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<MVCContrib.UI.ViewModels.EmailModel>" %>
to: foo@email.com
from: sender@email.com
subject: 
<html>
<body>
Greeting from <%= Model.From %>; sending to <%= Model.To %>.
</body>
</html>
