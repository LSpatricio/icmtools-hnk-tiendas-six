<%@ Page Title="Módulo Excepciones" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="Exceptions.aspx.vb" Inherits="ICMTools.Exceptions" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Form para Importación de Excepciones con procesos de validación"/>
    <meta name="author" content="Rousbelt Damian Garza Villarreal"/>
    <title>Módulo Excepciones</title>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<div class="container">
    <div class="row">
        <div class="col">
            <div class="list-group">
              <a href="../Pages/ExcepcionesDocumentacion.aspx" class="list-group-item list-group-item-action flex-column align-items-start">
                <div class="d-flex w-100 justify-content-between">
                  <h5 class="mb-1">Documentación</h5>
                  <i class="fas fa-book fa-fw"></i>
                </div>
                <p class="mb-1">Manual de ayuda sobre uso de módulo Excepciones, descarga y llenado de plantilla, carga, configuración y reporte.</p>
              </a>
              <a href="../Pages/ExcepcionesConfiguracion.aspx" class="list-group-item list-group-item-action flex-column align-items-start">
                <div class="d-flex w-100 justify-content-between">
                  <h5 class="mb-1">Configuración</h5>
                  <i class="fas fa-cogs fa-fw"></i>
                </div>
                <p class="mb-1">Listado de Conceptos permitidos y configuración para carga de Excepciones por Sociedad y CCNom de SAP.</p>
              </a>
              <a href="../Pages/ExcepcionesCarga.aspx" class="list-group-item list-group-item-action flex-column align-items-start">
                <div class="d-flex w-100 justify-content-between">
                  <h5 class="mb-1">Carga</h5>
                  <i class="fas fa-upload fa-fw"></i>
                </div>
                <p class="mb-1">Pantalla para importación masiva de Excepciones con Excel.</p>
              </a>
              <a href="../Pages/ExcepcionesReporte.aspx" class="list-group-item list-group-item-action flex-column align-items-start">
                <div class="d-flex w-100 justify-content-between">
                  <h5 class="mb-1">Reporte</h5>
                  <i class="fas fa-paperclip fa-fw"></i>
                </div>
                <p class="mb-1">Seguimientos de Cargas e historial por Periodo.</p>
              </a>
            </div>
        </div>
    </div>
</div>
</asp:Content>
