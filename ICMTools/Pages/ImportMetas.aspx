<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="ImportMetas.aspx.vb" Inherits="ICMTools.ImportMetas" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<%--Contenedor de botones en TopBar--%>
<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/ImportMetas.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/ImportMetasDocumentacion.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<%--Información de Modulo --%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Description" />
    <meta name="author" content="Author" />
    <title>Importación de Metas</title>

    <script type="text/javascript">
        const userEmail = "<%= CType(Session.Item("User"), ICMTools.User).Email %>";
        const serverPath = "<%= Page.Server.MapPath("~").Replace("\", "\\") %>";
        const pageConfig = {
            maxFileSize: "<%= ConfigurationManager.AppSettings("maxFileSize")%>",
            fileUploadSelector: "#<%= AsyncFileUpload1.ClientID %>",
            columns: ["IDSTORE", "PLAZA", "CR_PLAZA", "STORE_NAME", "CR_TIENDA", "GEN", "TAE"],
            types: ["String", "String", "String", "String", "String", "String", "String"],
            LogPage: "Importación de Metas",
            LogType: "Validacion",
            LogBody: "Inicia validación para carga de Importación de Metas",
            FileType: "VentaSugerida\\Metas",
            Extension: ".xlsx",
            apiUploadData: "/api/importmetas/uploaddata"
        };
        $(document).ready(function () {

            $('#btnStartImport').on('click', function (e) {
                e.preventDefault();
                Step1_CheckFile();
            });

            $('#btnStartImport').prop('disabled', true);
        });
    </script>
    <script src="../js/shared.js"></script>
    <script src="../js/ImportMetas.js?v=2"></script>
</asp:Content>

<%-- Contenedor principal --%>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-12">
                <div class="card bg-light card-danger">
                    <div class="card-header lead">Seleccione filtros y archivo a cargar</div>
                    <div class="card-body">
                        <form id="myForm" runat="server">
                            <div class="row">
                                <div class="col-md-7">
                                    <div class="row form-group">
                                        <label class="control-label col-sm-5 text-right">Sociedad</label>
                                        <div class="col-sm-7">
                                            <select id="SelectSociety" class="form-control form-control-sm" clientidmode="Static">
                                                <option value="-1">(!)TODAS</option>
                                            </select>
                                        </div>
                                    </div>
                                    <div class="row form-group">
                                        <label class="control-label col-sm-5 text-right">División de Personal</label>
                                        <div class="col-sm-7">
                                            <select id="SelectPersonnelDivision" class="form-control form-control-sm" clientidmode="Static">
                                                <option value="-1">(!)TODAS</option>
                                            </select>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-5">
                                    <div class="form-group">
                                        <label class="control-label col-sm-12">Archivo de Importación de Metas</label>
                                        <div class="col">
                                            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                                            <!-- Drop Zone -->
                                            <div class="upload-drop-zone" id="drop-zone">
                                                <ajaxToolkit:AsyncFileUpload ID="AsyncFileUpload1" runat="server" ThrobberID="myThrobber" OnClientUploadComplete="uploadComplete" OnClientUploadError="uploadError" OnClientUploadStarted="beforeUploadStarts" Width="100%" ErrorBackColor="#FFCCFF" CompleteBackColor="#CCFFCC" ForeColor="Black" />
                                            </div>
                                            <asp:Label runat="server" ID="myThrobber" Style="display: none;"><i class="fas fa-sync-alt fa-spin fa-fw"></i>Cargando archivo...</asp:Label>
                                            <div class="table-responsive">
                                                <table id="statusUploadTable" class="table table-bordered table-sm"></table>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </form>
                    </div>
                    <div class="card-footer">
                        <div class="row">
                            <div class="col">
                                <!-- Progress Bar -->
                                <div id="progressDiv" class="progress" style="display: none; height: 31px;">
                                    <div id="progressBar" class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%">
                                        <span class="sr-only">0%</span>
                                    </div>
                                </div>
                            </div>
                            <div class="col-5">
                                <button id="btnStartImport" class="btn btn-sm btn-primary float-right" data-toggle="tooltip" data-placement="top" title="Click aquí despues de elegir archivo de Importación de Metas"><i class="fas fa-upload fa-fw"></i>Iniciar Importación</button>
                                <a href="../TemplateFiles/ICMToolsPlantilla_Metas.xlsx" class="btn btn-sm btn-primary float-right mr-2" data-toggle="tooltip" data-placement="top" title="Descarga de Plantilla Importación de Metas para importación de Importación de Metas"><i class="fas fa-download fa-fw"></i>Descargar plantilla</a>
                            </div>
                        </div>
                    </div>
                    <!-- card footer -->
                </div>
            </div>
            <div class="col-12 mt-2">
                <div id="statusAlert" class="alert alert-warning fade" role="alert">
                    <i class="fas fa-exclamation-triangle fa-fw"></i><strong>Importante!</strong> Espere por favor, no actualice la página...
                </div>
            </div>
            <!--Tablas de Respuesta-->
            <div class="col-12" style="margin-top: -8px;">
                <div id="errorPanel" class="RespuestaPanel card border-danger" style="display: none;">
                    <div class="card-header text-danger lead">Detalle de problemas<span class="badge badge-danger float-right"><i class="fas fa-exclamation-circle fa-fw"></i>Problema</span></div>
                    <div class="card-body">
                        <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i><span id="fileNameError"></span></h5>
                        <p class="card-text" id="MensajeError">Por favor ajuste el archivo de acuerdo a los problemas detectados y vuelva a intentar la carga.</p>
                        <div id="formatErrors" class="pt-3 table-responsive text-default"></div>
                    </div>
                </div>
                <div id="successPanel" class="RespuestaPanel card border-success" style="display: none;">
                    <div class="card-header text-success lead">Confirmación de Carga Exitosa de Importación de Metas<span class="badge badge-success float-right"><i class="fas fa-check-circle fa-fw"></i>Listo</span></div>
                    <div class="card-body">
                        <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i><span id="fileNameSuccess"></span></h5>
                        <div id="formatSuccess" class="pt-3 table-responsive text-default"></div>
                    </div>
                </div>
                <div id="WarningPanel" class="RespuestaPanel card border-warning" style="display: none;">
                    <div class="card-header text-warning lead">Confirmación de carga parcial de Importación de Metas<span class="badge badge-warning float-right"><i class="fas fa-exclamation-circle fa-fw"></i>Advertencia</span></div>
                    <div class="card-body">
                        <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i><span id="fileNameWarning"></span></h5>
                        <div id="formatWarning" class="pt-3 table-responsive text-default">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src="../vendor/bootstrap-4.1.0/dist/js/bootstrap.min.js"></script>
    <script src="../vendor/bootstrap-filestyle-2.1.0/src/bootstrap-filestyle.min.js"></script>
</asp:Content>