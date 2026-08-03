<%@ Page Title="Home" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="Home.aspx.vb" Inherits="ICMTools.Home" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="" />
    <meta name="author" content="" />
    <title>Home</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid Home d-flex align-items-center justify-content-center">
        <div class="text-center">
            <img src="../images/logo_icmtools.png" alt="Logo ICM Tools" class="img-fluid" style="max-height: 300px;" />
            <h1 class="display-4 mt-4">Bienvenido a ICM Tools!</h1>
            <span>Portal de utilidad para apoyo de procesos en ICM Cognos.</span>
        </div>
    </div>
</asp:Content>

