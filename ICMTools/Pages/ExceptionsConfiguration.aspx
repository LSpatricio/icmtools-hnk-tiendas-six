<%@ Page Title="Configuración de Excepciones" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="ExceptionsConfiguration.aspx.vb" Inherits="ICMTools.ExceptionsConfiguration" %>
<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>

<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/ExceptionsUpload.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/ExceptionsConfiguration.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-cogs fa-2x"></i>
            <small>Configuración</small>
        </a>
        <a href="../Pages/ExceptionsReportHistory.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-history fa-2x"></i>
            <small>Historial de cargas</small>
        </a>
        <a href="../Pages/ExceptionsDocumentation.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Form para Importación de Excepciones con procesos de validación"/>
    <meta name="author" content="Rousbelt Damian Garza Villarreal"/>
    <title>Configuración de Excepciones</title>

    <script type="text/javascript" >
        const pageConfig = {
            FileType: "Excepciones",
            maxFileSize: "<%= ConfigurationManager.AppSettings("maxFileSize")%>",
            LogPage: "Excepciones Configuracion",
            LogType: "",
            LogBody: "",
            Society: $("#SelectSociety").val(),
            PersonnelDivision: $("#SelectPersonnelDivision").val(),
        };       
        $(document).ready(function () {
            $("#btnRefresh").click(function () {
                listExceptionsConfiguration();
            });
        });
    </script>
    <script src="../js/shared.js"></script>
    <script src="../js/Excepciones.js"></script>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-12">
            <div class="card bg-light card-danger">
              <div class="card-header lead">Seleccione filtros para buscar configuración</div>
              <div class="card-body">
                <form id="myForm" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <div class="row form-group">
                            <label class="control-label col-sm-5 text-right">Sociedad</label>
                            <div class="col-sm-7">
                                <select id="SelectSociety" onchange="$('#SelectPersonnelDivision').empty();loadPersonnelDivisionsBySociety();" class="form-control form-control-sm" runat="server" clientidmode="Static" autopostback="True"></select>
                            </div>
                        </div>
                        <div class="row form-group">
                            <label class="control-label col-sm-5 text-right">Division de personal</label>
                            <div class="col-sm-7">
                                <select id="SelectPersonnelDivision" class="form-control form-control-sm" runat="server" clientidmode="Static" autopostback="True"></select>
                            </div>
                        </div>
                    </div>

                </div>

                </form>  
              </div><!-- card body -->
        
              <div class="card-footer">
                  <div class="row">
                      <div class="col">
                          <label id="progressLabel" class="text-danger"></label>
                      </div>
                      <div class="col">
                        <button id="btnRefresh" class="btn btn-sm btn-primary float-right"><i class="fas fa-sync-alt fa-fw"></i>Mostrar configuración</button>
                      </div>
                  </div>
              </div><!-- card footer -->
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-12 mt-3">
            <div id="statusAlert" class="alert alert-warning fade" role="alert">
            </div>
        </div>
    </div>

        <div class="row">
            <div id="reportArea" class="col-12" style="display: none;">
                <div class="card border-info">
                  <div id="reportHeader" class="card-header lead">Configuración de conceptos de Excepciones</div>
                  <div class="card-body">
                    <h5 id="reportTitle" class="card-title text-info"></h5>
                    <div id="reportTable" class="card-text table-responsive"></div>
                  </div>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
