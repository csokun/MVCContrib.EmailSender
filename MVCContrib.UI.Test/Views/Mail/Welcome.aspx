<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<MVCContrib.SendMail.EmailMetadata>" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <h2>Hello</h2>
    <div>
      This is an HTML email.
    </div>
    <hr>
    This is a customizable message: <%= Model.CustomizableMessage %>
</asp:Content>
