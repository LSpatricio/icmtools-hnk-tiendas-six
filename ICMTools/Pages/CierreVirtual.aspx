<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="CierreVirtual.aspx.vb" Inherits="ICMTools.CierreVirtual" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<%--Contenedor de botones en TopBar--%>
<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/CierreVirtual.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/CierreVirtualDocumentacion.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>

    <script src="../js/shared.js"></script>
    <script src="../js/CierreVirtual.js?v=3"></script>
</asp:Content>

<%--Información de Modulo --%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Description" />
    <meta name="author" content="Author" />
    <title>Cierre Virtual</title>

    <script type="text/javascript">
        const pageConfig = {
            FileType: "",
            Extension: "",
            columns: ["Col1", "Col2", "Col3"],
            LogPage: "Cierre Virtual",
            LogType: "Consulta",
            LogBody: "Inicia validación para carga de Cierre Virtual"
        };
    </script>

    <style>
        /* 🎨 Encabezado guinda */
        .tabla-guinda thead th {
            background-color: #800000 !important; /* Fondo guinda */
            color: #ffffff !important; /* Letras blancas */
            text-align: center;
            vertical-align: middle;
        }

            .tabla-guinda thead th:hover {
                background-color: #660000 !important;
            }

        /* 🎨 Encabezado verde */
        .tabla-verde thead th {
            background-color: #006400 !important; /* Verde oscuro */
            color: #ffffff !important;
            text-align: center;
            vertical-align: middle;
        }

            .tabla-verde thead th:hover {
                background-color: #004d00 !important;
            }
    </style>
</asp:Content>

<%-- Contenedor principal --%>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <form id="myForm" runat="server">
        <div class="container mt-2">

            <!-- Texto alineado al centro -->
            <div class="row mb-4">
                <div class="col text-center ">
                    <h5 class="font-weight-bold text-dark">Cierre Virtual</h5>
                </div>
            </div>

            <!-- Texto alineado a la derecha -->
            <div class="row mb-2">
                <div class="col text-right">
                    <h6 class="font-weight-bold text-dark">
                        <small class="text-muted" id="hora_consulta"></small>
                    </h6>
                </div>
            </div>

            <div class="row">
                <div class="col-12 col-md-6 text-left">
                    Periodo:&nbsp;
                <asp:DropDownList ID="PeriodoSelect" runat="server" ClientIDMode="Static" CssClass="form-control form-control-sm d-inline-block" DataTextField="Periodo" DataValueField="Periodo" data-toogle="tooltip" data-placement="top" title="Selecciona el periodo a consultar" Style="max-width: 200px;" onchange="CleanUp();">
                </asp:DropDownList>
                </div>
                <div class="col-12 col-md-6 text-right">
                    <button id="DescargarButton" onclick="DescargarCierreVirtual(); return false;" class="btn btn-sm btn-primary" data-toggle="tooltip" data-placement="top" style="display: none;">
                        <i class="fa fa-download fa-fw"></i>Descargar
                    </button>
                    <button id="btnStartImport" onclick="ImportacionICM()" class="btn btn-sm btn-primary" data-toggle="tooltip" data-placement="top">
                        <i class="fas fa-play fa-fw"></i>Iniciar Consulta
                    </button>
                </div>
            </div>

            <div class="row mt-2">
                <div class="col-12 col-md-12">
                    <div id="progressDiv" class="progress" style="height: 31px;">
                        <div id="progressBar" class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%">
                            <span class="sr-only">0%</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-12 mt-2">
                <div id="statusAlert" class="alert alert-warning fade" role="alert">
                    <i class="fas fa-exclamation-triangle fa-fw"></i><strong>Importante!</strong> Espere por favor, no actualice la página...
                </div>
            </div>
            <!--Tablas de Respuesta-->
            <div class="col-12" style="margin-top: -8px;">
                <div id="errorPanel" class="card border-danger" style="display: none;">
                    <div class="card-header text-danger lead">Detalle de problemas<span class="badge badge-danger float-right"><i class="fas fa-exclamation-circle fa-fw"></i>Problema</span></div>
                    <div class="card-body">
                        <div id="formatErrors" class="pt-3 table-responsive text-default"></div>
                    </div>
                </div>
                <%--   <div id="successPanel" class="card border-success" style="display: none;">
                <div class="card-header text-success lead">Hecho!<span class="badge badge-success float-right"><i class="fas fa-check-circle fa-fw"></i>Listo</span></div>
                <div class="card-body">
                    <h5 class="card-title"><i class='fas fa-file-excel fa-fw'></i>Confirmación de Carga Exitosa de Cierre Virtual</h5>
                    <div id="formatSuccess" class="pt-3 table-responsive text-default"></div>
                </div>
            </div>--%>
            </div>

            <br />

            <!-- Tabla centrada -->
            <div class="row justify-content-center mb-2">
                <div class="col-md-6">
                    <table id="TableCierreVirtual" class="table table-striped table-bordered text-center tabla-guinda">
                        <thead class="thead-dark">
                            <tr>
                                <th scope="col">Nacional</th>
                                <th scope="col">Cerradas</th>
                                <th scope="col">Avance</th>
                            </tr>
                        </thead>
                        <tbody id="tbody_cierre_virtual">
                            <%--<asp:Repeater ID="rptCierreVirtual" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("Total") %></td>
                                    <td><%# Eval("CerradoSi") %></td>
                                    <td><%# Eval("Porcentaje") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>--%>
                        </tbody>
                    </table>
                </div>
            </div>

            <div class="row justify-content-center mb-2">
                <div class="col-md-4">
                    <table id="TableCierreVirtualPorcentaje" class="table text-center border-0">
                        <tbody id="tbody_cierre_virtual_porcentaje">
                            <%--<asp:Repeater ID="rptCierreVirtual_Porcentaje" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <tr>
                                        <td style='<%# 
                                        If(Convert.ToDouble(Eval("porcSi").ToString().Replace("%", "")) >= 90.0, 
                                           "background-color:#006400; color:white; font-weight:bold;", 
                                           "background-color:#800000; color:white; font-weight:bold;") 
                                    %>'>
                                            <%# Eval("porcSi") %>
                                        </td>

                                        <td style='<%# 
                                        If(Convert.ToDouble(Eval("porcSi").ToString().Replace("%", "")) >= 90.0, 
                                           "background-color:#006400; color:white; font-weight:bold;", 
                                           "background-color:#800000; color:white; font-weight:bold;") 
                                    %>'>
                                            <%# Eval("porcNo") %>
                                        </td>
                                    </tr>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>--%>
                        </tbody>
                    </table>
                </div>
            </div>

            <!-- Texto alineado a la derecha -->
            <div class="row mb-2">
                <div class="col text-right">
                    <h6 class="font-weight-bold text-dark">Generación de Documentos Oracle EBS</h6>
                </div>
            </div>

            <div class="row mb-2">
                <div class="col text-right">
                    <asp:Label runat="server" Text="Status de importación:"></asp:Label>
                    <label id="status_importacion"></label>
                </div>
            </div>

            <!-- Dos tablas lado a lado -->
            <div class="row">
                <div class="col-md-6">
                    <%--<h4 class="text-center">Tabla Izquierda</h4>--%>
                    <table id="TableDocIzquierda" class="table table-striped table-bordered text-center tabla-guinda">
                        <thead class="thead-dark">
                            <tr>
                                <th scope="col">Nombre Plaza</th>
                                <th scope="col">Tiendas</th>
                                <th scope="col">Cerradas</th>
                                <th scope="col">Avance</th>
                            </tr>
                        </thead>
                        <tbody id="tbody_cierre_por_plaza">
                            <%-- <asp:Repeater ID="rptCierrePorPlazaIzquierda" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("NombrePlaza") %></td>
                                    <td><%# Eval("Tiendas") %></td>
                                    <td><%# Eval("SI") %></td>
                                    <td>
                                        <%# If(Eval("Tiendas") IsNot Nothing AndAlso Convert.ToDouble(Eval("SI")) > 0,
                                                                          Math.Round((Convert.ToDouble(Eval("SI")) / Convert.ToDouble(Eval("Tiendas"))) * 100, 2) & "%", "0%") %>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>--%>
                        </tbody>
                    </table>
                </div>

                <div class="col-md-6">
                    <%--<h4 class="text-center">Tabla Derecha</h4>--%>
                    <table id="TableDocDerecha" class="table table-striped table-bordered text-center tabla-verde">
                        <thead class="thead-dark">
                            <tr>
                                <th scope="col">Nombre Plaza</th>
                                <th scope="col">Tiendas</th>
                                <th scope="col">Doc.Generados</th>
                                <th scope="col">Avance</th>
                            </tr>
                        </thead>
                        <tbody id="tbody_doc_generados">
                            <%--<asp:Repeater ID="rptXXICMGenDocumentos" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("NombrePlaza") %></td>
                                    <td><%# Eval("Tiendas") %></td>
                                    <td><%# Eval("SI") %></td>
                                    <td><%# If(Eval("Tiendas") IsNot Nothing AndAlso Convert.ToDouble(Eval("SI")) > 0,
                                  Math.Round((Convert.ToDouble(Eval("SI")) / Convert.ToDouble(Eval("Tiendas"))) * 100, 2) & "%", "0%") %>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>--%>
                        </tbody>
                    </table>

                    <div class="row mb-2">
                        <div class="col text-right">
                            <asp:Label Font-Bold="true" runat="server" Text="% de Avance:"></asp:Label>
                            <label id="doc_gen_percentage"></label>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Tabla centrada -->
            <div class="row justify-content-center mb-5">
                <div class="col-md-10">
                    <table id="TableCierreAvance" class="table table-striped table-bordered text-center  tabla-guinda">
                        <thead class="thead-dark">
                            <tr>
                                <th scope="col">Distrito</th>
                                <th scope="col">Nombre</th>
                                <th scope="col">Tiendas</th>
                                <th scope="col">Cerradas</th>
                                <th scope="col">Avance</th>
                            </tr>
                        </thead>
                        <tbody id="tbody_cierre_distritos">
                            <%--<asp:Repeater ID="rptCierrePorDistritoAbajo" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("DistritoID") %></td>
                                    <td><%# Eval("NombreDistrito") %></td>
                                    <td><%# Eval("Tiendas") %></td>
                                    <td><%# Eval("SI") %></td>
                                    <td><%# Eval("Porcentaje") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>--%>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <script src="../vendor/bootstrap-4.1.0/dist/js/bootstrap.min.js"></script>
        <script src="../vendor/bootstrap-filestyle-2.1.0/src/bootstrap-filestyle.min.js"></script>
    </form>
</asp:Content>
