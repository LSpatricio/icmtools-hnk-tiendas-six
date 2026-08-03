<%@ Page Title="Reporte de Excepciones" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="ExceptionsReportHistory.aspx.vb" Inherits="ICMTools.ExceptionsReportHistory" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>

<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/ExceptionsUpload.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/ExceptionsConfiguration.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-cogs fa-2x"></i>
            <small>Configuración</small>
        </a>
        <a href="../Pages/ExceptionsReportHistory.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
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
    <meta name="description" content="Form para Descargar plantilla e información de ayuda para el llenado"/>
    <meta name="author" content="Rousbelt Damian Garza Villarreal"/>
    <title>Reporte de Excepciones</title>

    <script type="text/javascript" >
        const pageConfig = {
            FileType: "Excepciones",
            maxFileSize: "<%= ConfigurationManager.AppSettings("maxFileSize")%>",
            LogPage: "Excepciones Histórico",
            LogType: "",
            LogBody: "",
            Period: $("#SelectPeriod").val(),
            Society: $("#SelectSociety").val(),
            PersonnelDivision: $("#SelectPersonnelDivision").val(),
        };        
        $(document).ready(function () {
            $("#btnRefresh").on('click', function (e) {
                e.preventDefault();
                getExceptionsHistoryReport();
            });
        });
    </script>
     <script src="../js/shared.js"></script>
    <script src="../js/Excepciones.js?v=2"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-12">
            <div class="card bg-light card-danger">
              <div class="card-header lead">Seleccione filtros para generar el reporte</div>
              <div class="card-body">
                <form id="myForm" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <div class="row form-group">
                            <label class="control-label col-sm-5 text-right">Periodo</label>
                            <div class="col-sm-7">
                                <select id="SelectPeriod"  class="form-control form-control-sm" runat="server" clientidmode="Static" autopostback="True"></select>
                            </div>
                        </div>
                        <div class="row form-group">
                            <label class="control-label col-sm-5 text-right">Sociedad</label>
                            <div class="col-sm-7">
                                <select id="SelectSociety" onchange="$('#SelectPersonnelDivision').empty();loadPersonnelDivisionsBySociety();" class="form-control form-control-sm" runat="server" clientidmode="Static" autopostback="True"></select>
                            </div>
                        </div>
                        <div class="row form-group">
                            <label class="control-label col-sm-5 text-right">División de Personal</label>
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
                        <button id="btnRefresh" class="btn btn-sm btn-primary float-right"><i class="fas fa-sync-alt fa-fw"></i> Mostrar reporte</button>
                      </div>
                  </div>
              </div><!-- card footer -->
            </div>
        </div>
        <div class="col-12">
            <h5 class="mt-2">Definición de estados de lotes</h5>
            <ul>
                <li><i class='fas fa-share-square'></i> CARGADO: El lote ha pasado las validaciones y se encuentra en espera de ser aplicado en ICM.</li>
                <li><i class='fas fa-lock-open fa-fw'></i> PROCESADO: Indica que el lote ya esta en ICM y se encuentra en espera de ser enviado a SAP.</li>
                <li><i class='fas fa-paper-plane fa-fw'></i> ENVIANDO: Indica que esta realizandose el envio a SAP.</li>
                <li><i class='fas fa-lock fa-fw'></i> ENVIADO A SAP: Este es el estado final, cuando las excepciones ya han sido aplicadas en SAP.</li>
                <li><i class='fas fa-exchange-alt'></i> REEMPLAZADO: El lote fue reemplazado en su totalidad por otro lote durante el proceso.</li>
            </ul>
        </div>

       </div>
       


        <div class="row">
            <div class="col-12 mt-3">
                <div id="statusAlert" class="alert alert-warning fade" role="alert">
                </div>
            </div>
            <div class="col-12" style="margin-top: -8px;">
                <div id="errorPanel" class="card border-danger" style="display: none;">
                    <div class="card-header text-danger lead">Detalle de problemas<span class="badge badge-danger float-right"><i class="fas fa-exclamation-circle fa-fw"></i>Problema</span></div>
                    <div class="card-body">
                        <p class="card-text">Ocurrió un error al generar el reporte</p>
                    </div>
                </div>
            </div>
        </div>        
        <div class="row">
            <div id="reportArea" class="col-12" style="display: none;">
                <div class="card border-info mb-3">
                    <div id="reportHeader" class="card-header lead">Historial de cargas de Excepciones</div>
                        <div class="card-body">
                            <h5 id="reportTitle" class="card-title text-info"></h5>
                            <div id="reportTable" class="card-text table-responsive"></div>
                    </div>
                </div>
            </div>            
        </div>
    </div>
<!-- Modal -->
<div class="modal fade bd-example-modal-lg" id="Modal" tabindex="-1" role="dialog">
  <div class="modal-dialog modal-lg" role="document">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title" id="ModalTitle">Modal title</h5>
        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
      <div class="modal-body table-responsive" id="ModalBody">

      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cerrar</button>
      </div>
    </div>
  </div>
</div>

</asp:Content>
