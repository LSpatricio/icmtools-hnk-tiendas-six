<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="EmpleadosActivosOracle.aspx.vb" Inherits="ICMTools.EmpleadosActivosOracle" %>
<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<%--Contenedor de botones en TopBar--%>
<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/EmpleadosActivosOracle.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/EmpleadosActivosOracleDocumentacion.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<%--Información de Modulo --%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Description" />
    <meta name="author" content="Author" />
    <title>Empleados Activos Oracle</title>

     <script type="text/javascript">
         const pageConfig = {
             LogPage: "Empleados Activos Oracle",
             LogType: "Validacion",
             LogBody: "Inicia validación para carga Empleados Activos Oracle",
         };
         $(document).ready(function () {
             $('#btnStartImport').on('click', function (e) {
                 e.preventDefault();
                 IniciaProceso();
             });

         });
     </script>
    <script src="../js/shared.js"></script>
    <script src="../js/EmpleadosActivosOracle.js"></script>
</asp:Content>

  <%-- Contenedor principal --%>
  <asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
      <div class="container">
          <div class="row">
              <div class="col-12">
                  <div class="card bg-light card-danger">
                      <div class="card-header lead">Seleccione filtros e inicie la ejecución</div>
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
                        <button id="btnStartImport" class="btn btn-sm btn-primary" data-toggle="tooltip" data-placement="top" title="Click aquí para iniciar el proceso de Empleados Activos Oracle" style="height: 3em; margin: 1.3em 4em;"><i class="fas fa-play fa-fw"></i> Iniciar Ejecución de Empleados Activos Oracle</button> 
                    </div>
                              </div>
                          </form>
                      </div>
                      <div class="card-footer">
                        <div class="row">
                            <div class="col">
                                <!-- Progress Bar -->
                                <div id="progressDiv" class="progress" style="display:none; height: 31px;">
                                    <div id="progressBar" class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%">
                                        <span class="sr-only">0%</span>
                                    </div>
                                </div>
                            </div>                         
                        </div>
                      </div><!-- card footer -->
                  </div>
              </div>
              <div class="col-12 mt-2">
                <div id="statusAlert" class="alert alert-warning fade" role="alert">
                    <i class="fas fa-exclamation-triangle fa-fw"></i><strong>Importante!</strong> Espere por favor, no actualice la página...
                </div>
              </div>
              <!--Tablas de Respuesta-->
              <div class="col-12" style="margin-top: -8px;">
                <div id="errorPanel" class="card border-danger" style="display:none;">
                    <div class="card-header text-danger lead">Detalle de problemas<span class="badge badge-danger float-right"><i class="fas fa-exclamation-circle fa-fw"></i>Problema</span></div>
                    <div class="card-body">
                        <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i><span id="fileNameError"></span></h5>
                        <p class="card-text">Por favor comuníquese ya que ocurrio una excepción no controlada.</p>
                        <div id="formatErrors" class="pt-3 table-responsive text-default"></div>
                    </div>
                </div>
                <div id="successPanel" class="card border-success" style="display:none;">
                    <div class="card-header text-success lead">Confirmación de ejecución exitosa de Empleados Activos Oracle<span class="badge badge-success float-right"><i class="fas fa-check-circle fa-fw"></i>Listo</span></div>
                        <div class="card-body">
                            <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i><span id="fileNameSuccess"></span></h5>
                                <div id="formatSuccess" class="pt-3 table-responsive text-default"></div>
                        </div>
                    </div>
                </div>
          </div>
      </div>
      <script src="../vendor/bootstrap-4.1.0/dist/js/bootstrap.min.js"></script>
      <script src="../vendor/bootstrap-filestyle-2.1.0/src/bootstrap-filestyle.min.js"></script>
  </asp:Content>