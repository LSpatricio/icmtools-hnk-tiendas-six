<%@ Page Title="Documentación: Entrada" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="MTTVEntradaDocumentacion.aspx.vb" Inherits="ICMTools.MTTVEntradaDocumentacion" %>
<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>

<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/MTTVEntrada.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/MTTVEntradaDocumentacion.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Form para Descargar plantilla e información de ayuda para el llenado"/>
    <meta name="author" content="Equipo SOINF"/>
    <title>Documentación de Entrada</title>

<style>
    body {
      position: relative !important;
  }
</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-md-4">
              <nav id="MyScrollNav" class="navbar navbar-expand-md navbar-light fixed bg-light flex-column sticky-top">
                <a class="navbar-brand" href="#">Manual de Ayuda</a>
                <nav class="nav nav-pills flex-column">
                  <a class="nav-link active" href="#item-1">Módulo de Entrada</a>
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
                    <a class="nav-link ml-3 my-1" href="#item-3-1">Validaciones de Archivo</a>

                  </nav>
                       </nav>
              </nav>
            </div>
            <div class="col-md-8">
                <h4 id="item-1">Módulo de Entrada</h4>
                <p>Módulo de ICM Tools creado en Octubre 2025 para uso del Modelo de Varicent ICM Cloud FEMCOEP.</p>
                <h5 id="item-1-1">Objetivo</h5>
                <p>Apoyar al usuario en la tarea de importar el archivo de Entrada de manera sencilla y rápida.</p>
                <h5 id="item-1-2">Acceso y Uso</h5>
                <p>El acceso a este módulo es exclusivo desde el portal ICMWeb y su uso es solo para FEMCOEP.</p>
                
                <h4 id="item-2">Plantilla</h4>
                <p>La plantilla es un archivo de Excel con la estructura necesaria para poder realizar la importación de Entrada desde la <a href="../Pages/MTTVEntrada.aspx">página de carga</a>.</p>
                <h5 id="item-2-1">Descarga</h5>
                <p>
                    Plantilla de referencia para iniciar el llenado (recomendable).
                    <a href="../TemplateFiles/ICMToolsPlantilla_MTTVEntrada.xlsx" class="btn btn-outline-info btn-sm"><i class="fas fa-download fa-fw"></i>Descargar archivo</a>
                </p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>Importante!</strong> La plantilla contiene los nombres de las columnas en la fila 1 y una fila con valores de Entrada, si utiliza este archivo es importante modifique la fila 2, ya que este es solo un ejemplo de como deben ser ingresadas las Entradas.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>

                <img class="mb-3" src="../images/Modulos/MultiTiendaVariable/Entrada/Ejemplo_Entrada.png?v=1"/>
                
                <h5 id="item-2-2">Archivo</h5>
                <p>Es importante tome en cuenta estos puntos al generar el archivo:</p>
                <ul>
                    <li>El archivo no deberá exceder el peso de 100 mb.</li>
                    <li>El nombre del archivo podrá ser modificado, para ICMTools es indistinto el nombre que usted decida utilizar.</li>
                    <li>El archivo debe ser extensión .xlsx o .csv (separado por comas).</li>
                    <li>El archivo deberá contener los registros de Entradas necesarios a partir de la fila 2.</li>
                </ul>

                <h4 id="item-3">Carga</h4>
                <p>Para iniciar la carga de Entrada deberá tener listo el archivo y acceder a la <a href="../Pages/MTTVEntrada.aspx">página de carga</a>.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/MultiTiendaVariable/Entrada/Pantalla_CargaEntrada.png?v=2"/>
                <p>Deberá seleccionar una opción en cada uno de los filtros y elegir el archivo que desea utilizar</p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                    <strong>¡Importante!</strong> En el caso de la pantalla de carga de Entrada no es necesario llenar las cajas de selección Sociedad y División de Personal.
                    <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>

                <ul>
                    <li><strong>Sociedad: </strong>Las opciones en este filtro mostrarán las sociedades a las que el usuario tiene acceso de acuerdo con la configuración dentro de ICM.</li>
                    <li><strong>División de Personal: </strong>Según la sociedad seleccionada, mostrará las divisiones de personal configuradas en las jerarquías en ICM.</li>
                    <li><strong>Archivo: </strong>Deberá hacer clic en el botón 'Elegir archivo' o arrastrar hasta la sección señalada el archivo de extensión .xlsx o .csv (separado por comas) a utilizar para la carga.</li>
                </ul>

                <p>Finalmente deberá dar clic en el botón "Iniciar Importación" para iniciar el proceso de carga.</p>

                <h5 id="item-3-1">Validaciones de Archivo</h5>
                <p>Las primeras validaciones que ICMTools realizará, serán en el formato del archivo Excel o CSV:</p>
                <ul>
                    <li>El archivo deberá de contener solo 8 columnas.</li>
                    <li>Sin tomar en cuenta la fila de los nombres de las columnas.</li>
                    <li>No se validarán mayúsculas o minúsculas, pero los nombres de las columnas deberán ser: CASOTABULADOR, CRPLAZA A, CRTIENDA_A, CRPLAZA_B, CRTIENDA_B, BEGDA, ENDDA y LGART.</li>
                    <li>Dentro del rango de celdas utilizado no deberán existir celdas vacías, en ninguna de las columnas.</li>
                    <li>No deberán existir registos duplicados, cada fila debe ser un registro único.</li>
                    <li>El formato Date debe ser DD/MM/YYYY, por ejemplo 31/10/2025.</li>
                </ul>
            </div>
        </div>
        
    </div>
</asp:Content>
