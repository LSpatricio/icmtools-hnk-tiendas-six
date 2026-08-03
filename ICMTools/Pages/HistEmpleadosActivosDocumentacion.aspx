<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="HistEmpleadosActivosDocumentacion.aspx.vb" Inherits="ICMTools.HistEmpleadosActivosDocumentacion" %>
<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<%--Contenedor de botones en TopBar--%>
<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/HistEmpleadosActivos.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/HistEmpleadosActivosDocumentacion.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<%--Información de módulo--%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Descripción" />
    <meta name="author" content="Autor" />
    <title>Histórico Empleados Activos Documentación</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script>
        $(document).ready(function () {
            $("#MyScrollNav a").on("click", function (e) {
                e.preventDefault();
                var target = $(this).attr("href");
                var offset = $(target).offset().top - 100;
                $(window).scrollTop(offset);
            });
        });
    </script>

    <div class="container">
        <div class="row">
            <div class="col-md-4">
              <nav id="MyScrollNav" class="navbar navbar-expand-md navbar-light fixed bg-light flex-column sticky-top">
                <a class="navbar-brand" href="#">
                    Manual de Ayuda
                </a>
                <nav class="nav nav-pills flex-column">
                  <a class="nav-link active" href="#item-1">Módulo Histórico Empleados Activos</a>
                  <nav class="nav nav-pills flex-column">
                    <a class="nav-link ml-3 my-1" href="#item-1-1">Objetivo</a>
                    <a class="nav-link ml-3 my-1" href="#item-1-2">Acceso y Uso</a>
                  </nav>
                  <a class="nav-link" href="#item-2">Plantilla</a>
                  <nav class="nav nav-pills flex-column">
                    <a class="nav-link ml-3 my-1" href="#item-2-1">Descarga</a>
                    <a class="nav-link ml-3 my-1" href="#item-2-2">Archivo</a>
                  </nav>
                  <a class="nav-link" href="#item-3">Carga</a>
                  <nav class="nav nav-pills flex-column">
                    <a class="nav-link ml-3 my-1" href="#item-3-1">Validaciones del archivo</a>
                  </nav>
                </nav>
              </nav>
            </div>
            <div class="col-md-8">    
                <h4 id="item-1">Módulo Histórico Empleados Activos</h4>
                <p>Módulo de ICM Tools creado en Octubre de 2025 para uso del modelo de Varicent ICM Cloud FEMCOVS.</p>

                <h5 id="item-1-1">Objetivo</h5>
                <p>Apoyar al usuario en la tarea de carga de Histórico Empleados Activos de manera sencilla y rápida.</p>

                <h5 id="item-1-2">Acceso y Uso</h5>
                <p>El acceso a este módulo es exclusivo desde el portal ICMWeb y su uso es solo para FEMCOVS.</p>
                
                <h4 id="item-2">Plantilla</h4>
                <p>La plantilla es un archivo de Excel con la estructura necesaria para poder realizar la carga de Histórico Empleados Activos desde la <a href="../Pages/HistEmpleadosActivos.aspx">página de carga</a>.</p>

                <h5 id="item-2-1">Descarga</h5>                
                <p>
                    Plantilla de referencia para iniciar el llenado (recomendable).
                    <a href="../TemplateFiles/ICMToolsPlantilla_HistEmpleadosActivos.xlsx" class="btn btn-outline-info btn-sm"><i class="fas fa-download fa-fw"></i>Descargar archivo</a>
                </p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>¡Importante!</strong> El archivo de plantilla contiene valores de prueba. Es fundamental que los reemplace por valores reales, ya que este archivo es solo un ejemplo de cómo debe cargarse la información. En este caso, la pantalla "Histórico Empleados Activos" utiliza dos archivos; la plantilla aplica para ambos casos.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>

                <img class="mb-3 img-fluid" src="../images/Modulos/HistEmpleadosActivos/Ejemplo_HistEmpleadosActivos.png?v=1"/>
                
                <h5 id="item-2-2">Archivo</h5>
                <p>Es importante que tome en cuenta estos puntos al generar el archivo.</p>
                <ul>
                    <li>El archivo no deberá exceder el peso de 100 MB.</li>
                    <li>El nombre del archivo podrá ser modificado, para ICMTools es indistinto el nombre que usted decida utilizar siempre y cuando al inicio contenga la palabra Inicial o Final, según sea el caso, seguido de un guíon bajo (Ejemplo: Inicial_HistoricoEmpleadosActivos.xlsx ó Final_HistoricoEmpleadosActivos.xlsx).</li>
                    <li>El archivo debe ser de extensión .xlsx o .csv (separado por comas).</li>
                    <li>El archivo deberá contener los registros que se necesitarán para la carga a partir de la fila 2.</li>
                </ul>

                <h4 id="item-3">Carga</h4>
                <p>Para iniciar la carga de Histórico Empleados Activos deberá tener listos los archivos necesarios y acceder a la <a href="../Pages/HistEmpleadosActivos.aspx">página de carga</a>.</p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                    <strong>¡Importante!</strong> En el caso de la pantalla de carga Histórico Empleados Activos, es necesario realizar la carga de <strong>dos archivos ya sean .xlsx o .csv (separado por comas).</strong> (Histórico Empleados Activos Inicial e Histórico Empleados Activos Final), ya que ambos son requeridos para el proceso de carga de datos.<br /><strong>Ambos archivos deben de ser del mismo tipo de dato</strong>, ya que no está permitida la carga de tipos de datos mezclados (Por ejemplo, que el archivo inicial sea .csv y el archivo final sea .xlsx)
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>
                <img class="mb-3 img-fluid" src="../images/Modulos/HistEmpleadosActivos/Pantalla_HistEmpleadosActivos.png?v=1"/>
                <p>Para realizar la carga de los archivos:</p>
                <ul>
                    <li><strong>Archivo: </strong>Deberá hacer clic en el botón 'Elegir archivo' o arrastrar hasta la sección señalada el archivo de extensión .xlsx o .csv (separado por comas) a utilizar para la carga.</li>
                </ul>
                <p>Finalmente, deberá dar clic en el botón "Iniciar Importación" para iniciar el proceso de carga.</p>

                <h5 id="item-3-1">Validaciones del archivo</h5>
                <p>Las primeras validaciones que ICMTools realizará serán en el formato del archivo:</p>
                <ul>
                    <li>El archivo deberá contener solo 11 columnas.</li>                    
                    <li>No se validarán mayúsculas ni minúsculas, pero los nombres de las columnas deberán ser: División de personal, Unidad organizativa, Sociedad, Número de personal, Nombre editado del empleado o candidato, Fecha de alta, Función, Centro de coste, CeCo Auxiliar, Subdivisión de personal, División.</li>
                    <li>Dentro del rango de celdas utilizado no deberán existir celdas vacías en ninguna de las columnas.</li>
                    <li>No deben existir registros duplicados; cada fila debe ser un registro único.</li>
                    <li>El formato de Fechas debe ser DD/MM/YYYY, por ejemplo, 28/05/2021.</li>
                </ul>

            </div>
        </div>
    </div>
</asp:Content>