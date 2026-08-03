<%@ Page Title="Documentación de Excepciones" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="ExceptionsDocumentation.aspx.vb" Inherits="ICMTools.ExceptionsDocumentation" %>

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
        <a href="../Pages/ExceptionsReportHistory.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-history fa-2x"></i>
            <small>Historial de cargas</small>
        </a>
        <a href="../Pages/ExceptionsDocumentation.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Form para Descargar plantilla e información de ayuda para el llenado"/>
    <meta name="author" content="Rousbelt Damian Garza Villarreal"/>
    <title>Documentación de Excepciones</title>

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
                  <a class="nav-link active" href="#item-1">Módulo Excepciones</a>
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
                    <a class="nav-link ml-3 my-1" href="#item-3-2">Validaciones ICM</a>
                  </nav>
                  <a class="nav-link" href="#item-4">Configuración</a>
                  <nav class="nav nav-pills flex-column">
                    <a class="nav-link ml-3 my-1" href="#item-4-1">Valores de configuración</a>
                  </nav>
                  <a class="nav-link" href="#item-5">Reportes</a>
                  <nav class="nav nav-pills flex-column">
                    <a class="nav-link ml-3 my-1" href="#item-5-1">Historial de cargas</a>
                    <a class="nav-link ml-3 my-1" href="#item-5-2">Estados de lotes</a>
                    <a class="nav-link ml-3 my-1" href="#item-5-3">Detalles de lote</a>
                  </nav>
                </nav>
              </nav>
            </div>
            <div class="col-md-8">
                <h4 id="item-1">Módulo Excepciones</h4>
                <p>Módulo de ICM Tools creado en Abril 2018 para uso del Modelo de ICM Cognos "FEMCOEPSAP" FEMCO Empleados Propios.</p>
                <h5 id="item-1-1">Objetivo</h5>
                <p>Apoyar al usuario en la tarea de Importar de manera sencilla pagos de Excepciones, además de poder mostrar un segumiento de las cargas previas mediante el Reporte por Periodo.</p>
                <h5 id="item-1-2">Acceso y Uso</h5>
                <p>El acceso a este módulo es excluisvo desde el portal ICMWeb y su uso es solo para FEMCOEPSAP.</p>
                
                <h4 id="item-2">Plantilla</h4>
                <p>La plantilla es un archivo de Excel con la estructura necesaria para poder realizar la importación de Excepciones desde la <a href="../Pages/ExceptionsUpload.aspx">página de carga</a>.</p>
                <h5 id="item-2-1">Descarga</h5>
                <p>
                    Plantilla de referencia para iniciar el llenado.
                    <a href="../TemplateFiles/ICMToolsPlantilla_Excepciones.xlsx" class="btn btn-outline-info btn-sm"><i class="fas fa-download fa-fw"></i>Descargar archivo</a>
                </p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>Importante!</strong> La plantilla contiene los nombres de las columnas en la fila 1 y una fila con valores de Excepciones, si utiliza este archivo es importante modifique la fila 2, ya que este es solo un ejemplo de como deben ser ingresadas las Excepciones.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>

                <img class="mb-3" src="../images/Modulos/Excepciones/layout_descarga.jpg?v=1"/>
                
                <h5 id="item-2-2">Archivo</h5>
                <p>Es importante tome en cuenta estos puntos al generar el archivo.</p>
                <ul>
                    <li>El archivo no debera exceder el peso de 100 mb.</li>
                    <li>El nombre del archivo podrá ser modificado, para ICMTools es indistinto el nombre que usted decida utilizar.</li>
                    <li>El archivo debe ser extension .xlsx</li>
                    <li>El archivo deberá contener los registros de Excepciones necesarios a partir de la fila 2.</li>
                </ul>

                <h4 id="item-3">Carga</h4>
                <p>Para iniciar la carga de Excepiones deberá tener listo el archivo y acceder a la <a href="../Pages/ExceptionsUpload.aspx">página de carga</a>.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/Excepciones/pantalla_carga.png?v=1"/>
                <p>Debera seleccionar una opción en cada unos de los filtros y seleccionar el archivo a utilizar:</p>
                <ul>
                    <li><strong>Periodo: </strong>De acuerdo a la fecha actual, se mostrarán los 2 periodos semanales anteriores.</li>
                    <li><strong>Sociedad: </strong>Las opciones en este filtro mostrarán las Sociedades a las que el usuario tiene acceso de acuerdo a la configuración dentro ICM.</li>
                    <li><strong>División de Personal: </strong>Según la Sociedad seleccionada, mostrará las Divisiones de Personal configurados en las Jerarquias en ICM.</li>
                    <li><strong>Archivo: </strong>Deberá dar clic en el boton o arrastrar hasta la sección señalada, el archivo de Excel a utilizar para la carga.</li>
                </ul>
                <p>Finalmente deberá dar clic en el boton "Importar Excepciones" para iniciar el proceso de carga.</p>

                <h5 id="item-3-1">Validaciones Excel</h5>
                <p>Las primeras validaciones que ICMTools realizará, seran en el formato del archivo Excel.</p>
                <ul>
                    <li>El archivo deberá de contener solo 5 columnas.</li>
                    <li>Sin tomar en cuenta la fila de los nombres de las columnas, el archivo no deberá de contener mas de 2000 registros.</li>
                    <li>No se validarán mayúsculas o minúsculas, pero los nombres de las columnas deberán ser: Empleado, Fecha, CCNom, Monto, Motivo.</li>
                    <li>Dentro del rango de celdas utilizado no deberán existir celdas vacias, en ninguna de las columnas.</li>
                    <li>No deberán existir registos duplicados, cada fila debe ser un registro unico de Excepción.</li>
                </ul>

                <h5 id="item-3-2">Validaciones ICM</h5>
                <p>La segunda parte de las validaciones son relacionadas contra la información de ICM.</p>
                <ul>
                    <li>El periodo seleccionado debe ser uno de los dos anteriores, de acuerdo a la fecha actual.</li>
                    <li>La Fecha de carga debe ser la "FechaFin" del Periodo + 1 día.</li>
                    <li>El Empleado debe estar vigente en ICM.</li>
                    <li>El Empleado debe pertenecer a la Sociedad seleccionada.</li>
                    <li>En caso de seleccionar una División de Personal que no sea genérica, se validará que el Empleado pertenezca a dicha División de Personal.</li>
                    <li>El CCNom debe estar activo para carga en la configuración de ICM.</li>
                    <li>Montos negativos solo serán permitidos si la configuración del CCNom lo permite.</li>
                    <li>El monto no deberá exceder el Monto Máximo, de acuerdo a la configuración del CCNom en ICM.</li>
                </ul>

                <h4 id="item-4">Configuración</h4>
                <p>Para revisar la configuración para carga de Excepciones deberá acceder a la <a href="../Pages/ExceptionsConfiguration.aspx">página de configuración</a>.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/Excepciones/pantalla_configuracion.png?v=1"/>
                <p>Debera seleccionar una opción en cada unos de los filtros:</p>
                <ul>
                    <li><strong>Sociedad:</strong> Las opciones en este filtro mostrarán las Sociedades a las que el Usuario tiene acceso de acuerdo a la configuración dentro ICM.</li>
                    <li><strong>División de Personal:</strong> Segun la Sociedad seleccionada, mostrará las Divisiones de Personal configurados en las Jerarquias en ICM.</li>
                </ul>
                <p>Finalmente deberá dar clic en el boton "Mostrar configuración" para ver la configuración para Carga de Excepciones en ICM.</p>

                <h5 id="item-4-1">Valores de configuración</h5>
                <p>Los valores que se muestran en el catálogo se encuentran dentro de la Base de Datos de ICM y en esta pantalla solo serán visibles como referencia para el usuario de ICMTools.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/Excepciones/pantalla_configuracion_ejemplo.png?v=1"/>
                <ul>
                    <li><strong>Sociedad: </strong>Sociedad configurado para la carga.</li>
                    <li><strong>División de Personal: </strong>División de Personal configurada para la carga.</li>
                    <li><strong>CCNom: </strong>Concepto CCNomina de SAP configurado para la carga.</li>
                    <li><strong>Valor máximo: </strong>Es el Mónto maximo permitido para la carga de Excepción.</li>
                    <li><strong>Permite negativo: </strong>Señala <i class='fas fa-flag text-success'></i> si se permite negativo, o <i class='fas fa-flag text-danger'></i> en caso de no permitir negativo.</li>
                    <li><strong>Activo: </strong>Señala <i class='fas fa-flag text-success'></i> si se encuentra activo, o <i class='fas fa-flag text-danger'></i> si esta deshabilitado para cargar.</li>
                </ul>


                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>Importante!</strong> En caso de requerir una modificación o alta de nuevo CCNom o Sociedad, deberá de ser reportado al equipo de Soporte ICM para proceder con la configuración necesaria.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>

                <h4 id="item-5">Reporte</h4>
                <p>Sección de reportes para vizualizar información relacionada con Excepciones.</p>

                <h5 id="item-5-1">Historial de cargas</h5>
                <p>Desde el reporte de <a href="../Pages/ExceptionsReportHistory.aspx">Historial de cargas</a>, podrá ver la información de los lotes de excepciones, ademas de rastrar el estado actual del lote.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/Excepciones/pantalla_reporte_historial.png?v=1"/>

                <h5 id="item-5-2">Estados de lotes</h5>
                <p>Despues de filtrar, Periodo, Sociedad y División de Personal podra ver el historial de cargas y el estado de cada lote.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/Excepciones/pantalla_reporte_historial_lotes.png?v=1"/>

                <p>Estos son los diferentes estados que se podrán vizualizar.</p>
                <ul>
                    <li><i class='fas fa-share-square'></i> CARGADO: El lote ha pasado las validaciones y se encuentra en espera de ser aplicado en ICM.</li>
                    <li><i class='fas fa-lock-open fa-fw'></i> PROCESADO: Indica que el lote ya esta en ICM y se encuentra en espera de ser enviado a SAP.</li>
                    <li><i class='fas fa-paper-plane fa-fw'></i> ENVIANDO: Indica que esta realizandose el envio a SAP.</li>
                    <li><i class='fas fa-lock fa-fw'></i> ENVIADO A SAP: Este es el estado final, cuando las excepciones ya han sido aplicadas en SAP.</li>
                    <li><i class='fas fa-exchange-alt'></i> REEMPLAZADO: El lote fue reemplazado en su totalidad por otro lote durante el proceso.</li>
                </ul>

                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong >Importante!</strong> En caso de tener alguna duda sobre el estatus actual de algun lote o su confirmación de pago, favor de reportarlo al equipo de soporte ICM haciendo referencia con el número del lote para poder rastrearlo.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>

                <h5 id="item-5-3">Detalles de lote</h5>
                <p>Para poder ver los detalles de las excepciones enviadas por lote, realice una busqueda con los filtros necesarios y despues haga clic en el botón <img class="mb-3 img-fluid" src="../images/Modulos/Excepciones/pantalla_reporte_historial_boton_detalles.png"/>, a continuación se mostrara en una pantalla emergente los registros de excepciones que contiene el lote.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/Excepciones/pantalla_reporte_historial_lotes_detalles.png?v=1"/>
                <p>Desde esta pantalla podrá hacer busquedas de información, copiar o exportar.</p>
                
                <img class="mb-3 img-fluid" src="../images/Modulos/Excepciones/pantalla_reporte_historial_botones_exportar.png?v=1"/>
                <p>Con el botón COPIAR, se realizara el <strong>copiado</strong> de los registros al "portapapeles" de Windows, despues de esto podra abrir el software de su preferenica y realizar un <strong>pegar</strong>.</p>
            </div>
        </div>
    </div>
</asp:Content>

