<%@ Page Title="Documentación de Tiendas" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="TiendasDocumentation.aspx.vb" Inherits="ICMTools.TiendasDocumentation" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<asp:Content ID="Content3" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="container-fluid">
        <div class="d-flex gap-1">
            <a href="../Pages/TiendasUpload.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
                <i class="fas fa-upload fa-2x"></i>
                <small>Carga</small>
            </a>
            <a href="../Pages/ExceptionsTiendasUpload.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
                <i class="fas fa-upload fa-2x"></i>
                <small>Carga de Excepciones</small>
            </a><a href="../Pages/TiendasDocumentation.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
                <i class="fas fa-book fa-2x"></i>
                <small>Documentación</small>
            </a>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Form para descargar plantilla e información de ayuda para el llenado" />
    <meta name="author" content="Equipo SOINF" />
    <title>Tiendas Documentación</title>
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
                    <a class="navbar-brand" href="#">Manual de Ayuda</a>
                    <nav class="nav nav-pills flex-column">
                        <a class="nav-link active" href="#item-1">Módulo Tiendas</a>
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
                            <a class="nav-link ml-3 my-1" href="#item-3-1">Validaciones Excel</a>
                        </nav>
                        <a class="nav-link active" href="#item-4">Módulo Excepciones de Tiendas</a>
                        <nav class="nav nav-pills flex-column">
                            <a class="nav-link ml-3 my-1" href="#item-4-1">Objetivo</a>
                            <a class="nav-link ml-3 my-1" href="#item-4-2">Acceso y Uso</a>
                        </nav>
                        <a class="nav-link" href="#item-5">Plantilla</a>
                        <nav class="nav nav-pills flex-column">
                            <a class="nav-link ml-3 my-1" href="#item-5-1">Descarga</a>
                            <a class="nav-link ml-3 my-1" href="#item-5-2">Archivo</a>
                        </nav>
                        <a class="nav-link" href="#item-6">Carga</a>
                        <nav class="nav nav-pills flex-column">
                            <a class="nav-link ml-3 my-1" href="#item-6-1">Validaciones Excel</a>
                        </nav>
                    </nav>
                </nav>
            </div>
            <div class="col-md-8">
                <h4 id="item-1">Módulo Tiendas</h4>
                <p>Módulo de ICM Tools creado en Octubre de 2025 para uso del modelo de Varicent ICM Cloud FEMCOEP.</p>
                <h5 id="item-1-1">Objetivo</h5>
                <p>Apoyar al usuario en la tarea de importar Tiendas de manera sencilla y rapida.</p>
                <h5 id="item-1-2">Acceso y Uso</h5>
                <p>El acceso a este módulo es excluisvo desde el portal ICMWeb y su uso es solo para FEMCOEP.</p>
                <h4 id="item-2">Plantilla</h4>
                <p>La plantilla es un archivo de Excel con la estructura necesaria para poder realizar la importación de Tiendas desde la <a href="../Pages/TiendasUpload.aspx">página de carga</a>.</p>
                <h5 id="item-2-1">Descarga</h5>
                <p>
                    Plantilla de referencia para iniciar el llenado (Recomendable).
                    <a href="../TemplateFiles/ICMToolsPlantilla_Tiendas.xlsx" class="btn btn-outline-info btn-sm"><i class="fas fa-download fa-fw"></i>Descargar archivo</a>
                </p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                    <strong>¡Importante!</strong> El archivo plantilla contiene valores de prueba, es importante que modifique sus valores por valores reales, ya que este es solo un ejemplo de cómo debe ser cargada la información.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                      <span aria-hidden="true">&times;</span>
                  </button>
                </div>
                <img class="mb-3 img-fluid" src="../images/Modulos/Tiendas/Ejemplo_Tiendas.png?v=1" />
                <h5 id="item-2-2">Archivo</h5>
                <p>Es importante tome en cuenta estos puntos al generar el archivo.</p>
                <ul>
                    <li>El archivo no deberá exceder el peso de 100 MB.</li>
                    <li>El nombre del archivo podrá ser modificado; para ICMTools es indistinto el nombre que usted decida utilizar.</li>
                    <li>El archivo debe ser de extensión .xlsx o .csv (separado por comas).</li>
                    <li>El archivo deberá contener los registros que se necesitarán para la carga a partir de la fila 2.</li>
                </ul>
                <h4 id="item-3">Carga</h4>
                <p>Para iniciar la carga de Tiendas deberá tener listo el archivo y acceder a la <a href="../Pages/TiendasUpload.aspx">página de carga</a>.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/Tiendas/Pantalla_Tiendas.png?v=2" />
                <p>Debera seleccionar una opción en cada unos de los filtros y seleccionar el archivo a utilizar (Opcional):</p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                    <strong>¡Importante!</strong> En el caso de la pantalla de carga Tiendas no es necesario llenar las cajas de selección Sociedad y División de Personal.
                    <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <ul>
                    <li><strong>Sociedad: </strong>Las opciones en este filtro mostrarán las Sociedades a las que el usuario tiene acceso de acuerdo a la configuración dentro ICM.</li>
                    <li><strong>División de Personal: </strong>Según la Sociedad seleccionada, mostrará las Divisiones de Personal configuradas en las Jerarquias en ICM.</li>
                    <li><strong>Archivo: </strong>Deberá dar clic en el botón o arrastrar hasta la sección señalada, el archivo de Excel a utilizar para la carga.</li>
                </ul>
                <p>Finalmente, deberá dar clic en el botón "Iniciar Importación" para iniciar el proceso de carga.</p>
                <h5 id="item-3-1">Validaciones Excel</h5>
                <p>Las primeras validaciones que ICMTools realizará, seran en el formato del archivo Excel.</p>
                <ul>
                    <li>El archivo deberá de contener solo 6 columnas.</li>
                    <li>No se validarán mayúsculas o minúsculas, pero los nombres de las columnas deberán ser: Sociedad, Division, CR Tienda, Fecha Inicio, Fecha Fin, Monto Diario.</li>
                    <li>Dentro del rango de celdas utilizado no deberán existir celdas vacias, en ninguna de las columnas.</li>
                    <li>No deben existir registros duplicados; cada fila debe ser un registro único.</li>
                    <li>El formato Fecha Inicio y Fecha Fin debe ser MM/DD/YYYY, por ejemplo 05/28/2021.</li>
                </ul>
                <div>
                    <hr />
                </div>
                <h4 id="item-4">Módulo Excepciones de Tiendas</h4>
                <p>Módulo de ICM Tools creado en Octubre de 2025 para uso del modelo de Varicent ICM Cloud FEMCEP.</p>
                <h5 id="item-4-1">Objetivo</h5>
                <p>Apoyar al usuario en la tarea de Importar Excepciones de Tiendas de manera sencilla y rapida.</p>
                <h5 id="item-4-2">Acceso y Uso</h5>
                <p>El acceso a este módulo es excluisvo desde el portal ICMWeb y su uso es solo para FEMCOEP.</p>
                <h4 id="item-5">Plantilla</h4>
                <p>La plantilla es un archivo de Excel con la estructura necesaria para poder realizar la importación de Tiendas desde la <a href="../Pages/ExceptionsTiendasUpload.aspx">página de carga</a>.</p>
                <h5 id="item-5-1">Descarga</h5>
                <p>
                    Plantilla de referencia para iniciar el llenado (Recomendable).
                    <a href="../TemplateFiles/ICMToolsPlantilla_ExcepcionTiendas.xlsx" class="btn btn-outline-info btn-sm"><i class="fas fa-download fa-fw"></i>Descargar archivo</a>
                </p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>¡Importante!</strong> El archivo plantilla contiene valores de prueba, es importante que modifique sus valores por valores reales, ya que este es solo un ejemplo de cómo debe ser cargada la información.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>
                <img class="mb-3 img-fluid" src="../images/Modulos/TiendasExcepciones/Ejemplo_ExcepcionesTiendas.png?v=1" />
                <h5 id="item-5-2">Archivo</h5>
                <p>Es importante tome en cuenta estos puntos al generar el archivo.</p>
                <ul>
                    <li>El archivo no debera exceder el peso de 100 mb.</li>
                    <li>El nombre del archivo podrá ser modificado, para ICMTools es indistinto el nombre que usted decida utilizar.</li>
                    <li>El archivo debe ser de extensión .xlsx o .csv (separado por comas).</li>
                    <li>El archivo deberá contener los registros de Excepciones de Tiendas necesarios a partir de la fila 2.</li>
                </ul>
                <h4 id="item-6">Carga</h4>
                <p>Para iniciar la carga de Excepciones de Tiendas deberá tener listo el archivo y acceder a la <a href="../Pages/ExceptionsTiendasUpload.aspx">página de carga</a>.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/TiendasExcepciones/Pantalla_ExcepcionesTiendas.png?v=1" />
                <p>Debera seleccionar una opción en cada unos de los filtros y seleccionar el archivo a utilizar:</p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>¡Importante!</strong> En el caso de la pantalla de carga Excepciones de Tiendas no es necesario llenar las cajas de selección Sociedad y División de Personal.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                      <span aria-hidden="true">&times;</span>
                  </button>
                </div>
                <ul>
                    <li><strong>Sociedad: </strong>Las opciones en este filtro mostrarán las Sociedades a las que el usuario tiene acceso de acuerdo a la configuración dentro ICM.</li>
                    <li><strong>División de Personal: </strong>Según la Sociedad seleccionada, mostrará las Divisiones de Personal configuradas en las Jerarquias en ICM.</li>
                    <li><strong>Archivo: </strong>Deberá dar clic en el botón o arrastrar hasta la sección señalada, el archivo de Excel a utilizar para la carga.</li>
                </ul>
                <p>Finalmente, deberá dar clic en el botón "Iniciar Importación" para iniciar el proceso de carga.</p>
                <h5 id="item-6-1">Validaciones Excel</h5>
                <p>Las primeras validaciones que ICMTools realizará, seran en el formato del archivo Excel.</p>
                <ul>
                    <li>El archivo deberá de contener solo 7 columnas.</li>
                    <li>No se validarán mayúsculas o minúsculas, pero los nombres de las columnas deberán ser: Sociedad, Division, CR Tienda, No Empleado, Fecha Inicio, Fecha Fin, Monto Diario.</li>
                    <li>Dentro del rango de celdas utilizado no deberán existir celdas vacias, en ninguna de las columnas.</li>
                    <li>No deben existir registros duplicados; cada fila debe ser un registro único.</li>
                    <li>El formato Fecha Inicio y Fecha Fin debe ser MM/DD/YYYY, por ejemplo 05/28/2021.</li>
                </ul>
            </div>
        </div>
    </div>
</asp:Content>
