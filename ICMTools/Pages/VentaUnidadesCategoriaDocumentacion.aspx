<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="VentaUnidadesCategoriaDocumentacion.aspx.vb" Inherits="ICMTools.VentaUnidadesCategoriaDocumentacion" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<%--Contenedor de botones en TopBar--%>
<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/VentaUnidadesCategoria.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/VentaUnidadesCategoriaDocumentacion.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<%--Información de módulo--%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Descripción" />
    <meta name="author" content="Autor" />
    <title>Venta Unidades Documentación</title>
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
                  <a class="nav-link active" href="#item-1">Módulo</a>
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

                <h4 id="item-1">Módulo Venta Unidades</h4>
                <p>Módulo de ICM Tools creado en Octubre de 2025 para uso del modelo de Varicent ICM Cloud FEMCOVS.</p>

                <h5 id="item-1-1">Objetivo</h5>
                <p>Apoyar al usuario en la tarea de carga de Venta Unidades de manera sencilla y rápida.</p>

                <h5 id="item-1-2">Acceso y Uso</h5>
                <p>El acceso a este módulo es exclusivo desde el portal ICMWeb y su uso es solo para FEMCOVS.</p>
                
                <h4 id="item-2">Plantilla</h4>
                <p>La plantilla es un archivo de Excel con la estructura necesaria para poder realizar la carga de Venta Unidades desde la <a href="../Pages/VentaUnidadesCategoria.aspx">página de carga</a>.</p>

                <h5 id="item-2-1">Descarga</h5>                
                <p>
                    Plantilla de referencia para iniciar el llenado (recomendable).
                    <a href="../TemplateFiles/ICMToolsPlantilla_VentaUnidades_118A.xlsx" class="btn btn-outline-info btn-sm"><i class="fas fa-download fa-fw"></i>Descargar archivo</a>
                </p>

                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>¡Importante!</strong> El archivo plantilla contiene valores de prueba, es importante que modifique sus valores por valores reales, ya que este es solo un ejemplo de cómo debe ser cargada la información.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>

                <img class="mb-3 img-fluid" src="../images/Modulos/Categoria/Ejemplo_VentaUnidades.png?v=1"/>
                
                <h5 id="item-2-2">Archivo</h5>
                <p>Es importante tomar en cuenta estos puntos al generar el archivo:</p>
                <ul>
                    <li>El archivo no deberá exceder el peso de 100 MB.</li>
                    <li>El archivo debe ser de extensión .xlsx o .csv (separado por comas).</li>
                    <li>El archivo deberá contener los registros que se necesitarán para la carga a partir de la fila 2.</li>
                    <li>El nombre del archivo podrá ser modificado, para ICMTools es indistinto el nombre que usted decida utilizar. Siempre y cuando al final contenga un guion bajo y una clave de CCNomina (Ejemplo: VentaUnidades_118A.xlsx).</li>
                </ul>

                <h4 id="item-3">Carga</h4>
                <p>Para iniciar la carga de Venta Unidades deberá tener listo el archivo y acceder a la <a href="../Pages/VentaUnidadesCategoria.aspx">página de carga</a>.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/Categoria/Pantalla_VentaUnidades.png?v=3"/>

                <p>Deberá seleccionar una opción en cada uno de los filtros y elegir el archivo a utilizar:</p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>¡Importante!</strong> En el caso de la pantalla de carga Venta Unidades no es necesario llenar las cajas de selección Sociedad y División de Personal.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>
                <ul>
                    <li><strong>Sociedad: </strong>Las opciones en este filtro mostrarán las sociedades a las que el usuario tiene acceso de acuerdo con la configuración dentro de ICM.</li>
                    <li><strong>División de Personal: </strong>Según la sociedad seleccionada, mostrará las divisiones de personal configuradas en las jerarquías en ICM.</li>
                    <li><strong>Archivo: </strong>Deberá dar clic en el botón o arrastrar hasta la sección señalada el archivo de Excel a utilizar para la carga.</li>
                </ul>

                <p>Finalmente, deberá dar clic en el botón "Iniciar Importación" para iniciar el proceso de carga.</p>

                <h5 id="item-3-1">Validaciones del archivo</h5>
                <p>Las primeras validaciones que ICMTools realizará serán en el formato del archivo:</p>
                <ul>
                    <li>El archivo deberá contener solo 10 columnas.</li>                    
                    <li>No se validarán mayúsculas ni minúsculas, pero los nombres de las columnas deberán ser: IDStore, CATEGORIA_DES, CR_PLAZ, PLAZA, CR_TIENDA, TIENDA, ID_USUARIO, ID_EMPLEADO, udsnetas, creationDate.</li>
                    <li>Dentro del rango de celdas utilizado no deberán existir celdas vacías en ninguna de las columnas.</li>
                    <li>El formato de fechas debe ser DD/MM/YYYY, por ejemplo, 16/10/2025.</li>
                </ul>
            </div>
        </div>
    </div>
</asp:Content>
