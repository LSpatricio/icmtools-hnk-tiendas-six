<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="BonosDocumentation.aspx.vb" Inherits="ICMTools.BonosDocumentation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <meta name="description" content="Form para Documentacion de Carga de Bonos de Transporte"/>
     <meta name="author" content="Donato Almiray"/>
 <title>Documentación de Excepciones</title>

 <style>
    body {
      position: relative !important;
  }

</style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="TopbarContent" runat="server">
   <div class="container-fluid">
<div class="d-flex gap-1">
      <a href="../Pages/BonosUpload.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
        <i class="fas fa-upload fa-2x"></i>
        <small>Carga</small>
      </a>  
      
      <a href="../Pages/BonosAuthorization.aspx"  class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
        <i class="fas fa-check-square fa-2x"></i>
        <small>Autorización</small>
      </a>

    <a href="../Pages/BonosDocumentation.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
  <i class="fas fa-book fa-2x"></i>
  <small>Documentación</small>
</a>
    </div>
</div>

</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
    <div class="row">
        <div class="col-md-4">
          <nav id="MyScrollNav" class="navbar navbar-expand-md navbar-light fixed bg-light flex-column sticky-top">
            <a class="navbar-brand" href="#">
                Manual de Ayuda<br />
                Bonos de Transporte
            </a>
            <nav class="nav nav-pills flex-column">
              <a class="nav-link active" href="#item-1">Módulo Carga de Bonos de Transporte</a>
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
                <a class="nav-link ml-3 my-1" href="#item-3-1">Nueva Carga</a>
                <a class="nav-link ml-3 my-1" href="#item-3-2">Validaciones Excel</a>
                <a class="nav-link ml-3 my-1" href="#item-3-3">Validaciones de Carga</a>
                <a class="nav-link ml-3 my-1" href="#item-3-4">Consulta Carga</a>

              </nav>
            </nav>
              <a class="navbar-brand" href="#">
                  Manual de Ayuda<br />
                  Autorización de Bonos de Transporte
              </a>
              <nav class="nav nav-pills flex-column">
              <a class="nav-link active" href="#item-4">Módulo Autorización de Bonos</a>
              <nav class="nav nav-pills flex-column">
                <a class="nav-link ml-3 my-1" href="#item-4-1">Objetivo</a>
                <a class="nav-link ml-3 my-1" href="#item-4-2">Acceso y Uso</a>
              </nav>
              <a class="nav-link" href="#item-5">Autorizar</a>
              <nav class="nav nav-pills flex-column">
               <a class="nav-link ml-3 my-1" href="#item-5-1">Autorizar Carga</a>
               <a class="nav-link ml-3 my-1" href="#item-5-2">Consultar Carga Cerrada</a>
               
             </nav>
           
            </nav>
          </nav>
        </div>
        <div class="col-md-8">            

            <h4 id="item-1">Módulo de Carga de Bonos</h4>
            <p>Módulo de ICM Tools creado en Julio 2025 para uso del Modelo de ICM Cognos "FEMCOEPSAP" FEMCO.</p>
            <h5 id="item-1-1">Objetivo</h5>
            <p>Apoyar al usuario en la tarea de Importar Bonos de Transporte de manera sencilla y rapida.</p>
            <h5 id="item-1-2">Acceso y Uso</h5>
            <p>El acceso a este módulo es excluisvo desde el portal ICMWeb y su uso es solo para FEMCOEPSAP.</p>
            
            <h4 id="item-2">Plantilla</h4>
            <p>La plantilla es un archivo de Excel con la estructura necesaria para poder realizar la importación de los Bonos de Transporte desde la <a href="../Pages/BonosUpload.aspx">página de carga</a>.</p>
            <h5 id="item-2-1">Descarga</h5>
            <p>
                Plantilla de referencia para iniciar el llenado (Recomendable).
                <a href="../TemplateFiles/ICMToolsPlantilla_BonosTransporte.xlsx" class="btn btn-outline-info btn-sm"><i class="fas fa-download fa-fw"></i>Descargar archivo</a>
            </p>
            <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
              <strong>Importante!</strong> La plantilla contiene los nombres de las columnas en la fila 1 y dos filas con valores de Tiendas, si utiliza este archivo es importante modifique la fila 2 y 3, ya que este es solo un ejemplo de como deben ser ingresados los Bonos.
              <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>

            <img class="mb-3 img-fluid" src="../images/Modulos/BonosTransportes/layout_carga_BonosTransporte.png?v=1"/>
            
            <h5 id="item-2-2">Archivo</h5>
            <p>Es importante tome en cuenta estos puntos al generar el archivo.</p>
            <ul>
                <li>El archivo no debera exceder el peso de 100 mb.</li>
                <li>El nombre del archivo podrá ser modificado, para ICMTools es indistinto el nombre que usted decida utilizar.</li>
                <li>El archivo debe ser extension .xlsx</li>
                <li>El archivo deberá contener los registros de Tiendas necesarios a partir de la fila 2.</li>
            </ul>

            <h4 id="item-3">Carga</h4>
            <p>Para iniciar la carga de los Bonos de Transporte deberá tener listo el archivo y acceder a la <a href="../Pages/BonosUpload.aspx">página de carga</a>.</p>
            <img class="mb-3 img-fluid" src="../images/Modulos/BonosTransportes/pantalla_BonosTransporte_Carga_v2.png?v=1"/>
            <p>Se mostrarán las diferentes cargas(lotes) realizadas con su respectivo estatus, ordenados primero los que están en estatus “EN PROCESO” del más antiguo al más reciente.
               Se tendra un filtro de estatus de la carga, los cuales son:
            </p>
                
              
            <ul>
                <li><strong>EN PROCESO: </strong>Estatus inicial después de realizar la carga.</li>
                <li><strong>CERRADO: </strong>Estatus que si el lote fue terminado de autorizar o fue rechazado</li>                
            </ul>


             <h5 id="item-3-1">Nueva Carga</h5>
            <p> Para iniciar una nueva carga se debera dar click al botón "Cargar Bonos Transporte", el cual abre la siguiente ventana: </p>
            <img class="mb-3 img-fluid" src="../images/Modulos/BonosTransportes/pantalla_BonosTransporte_Carga_Modal_v2.png?v=1"/>

             <p>Debera seleccionar una opción en cada unos de los filtros y seleccionar el archivo a utilizar:</p>
             <ul>
                <li><strong>Sociedad: </strong>Las opciones en este filtro mostrarán las Sociedades a las que el usuario tiene acceso de acuerdo a la configuración dentro ICM.</li>
                <li><strong>División: </strong>Según la Sociedad seleccionada, mostrará las Divisiones de Personal configuradas en las Jerarquias en ICM.</li>              
                <li><strong>Periodo: </strong>Listado de periodos a la que pertenece el lote de carga, se mostraran 3 periodos:  actual, anterior y posterior.</li>
    
                 <li><strong>Elige Archivo: </strong>Deberá dar clic en el botón o arrastrar hasta la sección señalada, el archivo de Excel a utilizar para la carga.</li>
             </ul>

            <h5 id="item-3-2">Validaciones Excel</h5>
            <p>Las primeras validaciones que ICMTools realizará, serán: </p>
            <ul>
                <li>El archivo deberá de contener solo 5 columnas.</li>
                <li>Sin tomar en cuenta la fila de los nombres de las columnas, el archivo no deberá de contener mas de 50000 registros.</li>
                <li>No se validarán mayúsculas o minúsculas, pero los nombres de las columnas deberán ser: Empleado, Fecha, CCNom, Monto, Motivo.</li>
                <li>Dentro del rango de celdas utilizado no deberán existir celdas vacias, en ninguna de las columnas.</li>          
                <li>El formato Fecha debe ser DD/MM/YYYY, por ejemplo 28/05/2021.</li>
                <li>El Empleado debera estar activo.</li>
                <li>La Fecha debera estar dentro del Periodo seleccionado en el filtro superior.</li>
                <li>El CCNom debera estar activo.</li>
                <li>Se necesita autorización si el Monto máximo es mayor al valor configurado en los parámetros globales, en caso de ser menor no requiere autorización. </li>

            </ul>

            <p>Despues de validar el documento, se mostraran los registros con su respectivo estatus. en las 3 secciones diferentes:</p>
             <ul>
                 <li><strong>Registros sin autorización:</strong> Son los registros que NO requieren una autorizacion para ser enviados a ICM.</li>
                 <li><strong>Registros con autorización:</strong> Son los registros que SI requieren una autorizacion para ser enviados a ICM.</li>
                 <li><strong>Registros excluidos:</strong> Son los registros que no son validos y no serán enviados a ICM.</li>
             </ul>

             <p>Finalmente deberá dar clic en el boton "Generar Carga" para iniciar el proceso de carga.</p>

             <h5 id="item-3-3">Validaciones de Carga</h5>
             <p>El proceso realizara las siguientes validaciones:</p>
             <ul>
                 <li>Debe existir un autorizador disponible, en caso de que no exista se debe configurar un reemplazo en el modulo de reemplazos. </li>
                 <li>Se valida los registros con error, en caso de que haya registros con ese estatus, no seran tomados en cuenta para el envio a ICM. </li>
             </ul>

             <p>Si se termina el proceso de carga de forma correcta, se generará un correo de aviso al autorizador.</p>

             <h5 id="item-3-4">Consultar Carga</h5>
            <p>Si se requiere consultar alguna carga, se pulsa la opcion en su columna <strong>Evento</strong> de la lista de cargas visualizadas.</p>
             <img class="mb-3 img-fluid" src="../images/Modulos/BonosTransportes/pantalla_BonosTransporte_Carga_v2.png?v=1"/>
            
            <p>Nos abrira la siguiente pantalla a modo de consulta (NO EDITABLE):</p>
            <img class="mb-3 img-fluid" src="../images/Modulos/BonosTransportes/pantalla_Consultar_Carga_BonoTransporte.png?v=1"/>

            <p>Mostrara la información guardada, con la siguiente clasificación:</p>
            <ul>
                  <li><strong>Registros Cargados:</strong> Son los registros si son válidos y/o fueron autorizados para ser enviados a ICM.</li>
                  <li><strong>Registros Rechazados:</strong> Son los registros que NO fueron autorizados para ser enviados a ICM.</li>
                  <li><strong>Registros excluidos:</strong> Son los registros que NO son validos y no fueron enviados a ICM.</li>
            </ul>

             <p>Asi como los comentarios adicionales que hayan sido capturados al momento de cargar el Lote.</p>

            <div>        
                <hr /> 
            </div>
                        
             <h4 id="item-4">Módulo Autorización de Bonos de Transporte</h4>
            <p>Módulo de ICM Tools creado en Julio 2025 para uso del Modelo de ICM Cognos "FEMCOEPSAP" FEMCO.</p>
            <h5 id="item-4-1">Objetivo</h5>
            <p>Apoyar al usuario en la tarea de Autorizar las cargas de los Bonos de transporte de manera sencilla y rapida.</p>
            <h5 id="item-4-2">Acceso y Uso</h5>
            <p>El acceso a este módulo es excluisvo desde el portal ICMWeb y su uso es solo para FEMCOEPSAP.</p>
            

             <h4 id="item-5">Autorizar</h4>
              <p>Pantalla para realizar la autorización de las cargas realizadas para Bonos de Transporte.  </p>
             <h5 id="item-5-1">Autorizar Carga</h5>
             
             <img class="mb-3 img-fluid" src="../images/Modulos/BonosTransportes/pantalla_BonosTransporte_Autorizacion_v2.png?v=1"/>
             <p>Se visualizaran las cargas realizadas para Bonos de Transporte que esten con Estatus "EN PROCESO" y que esten pendientes de Autorizar,
                 ordenadas por la fecha mas antigua.  </p>
              <p>   
                  Se tendra un filtro de estatus de la carga, los cuales son:
             </p>

              <ul>
                  <li><strong>En proceso: </strong>Estatus inicial después de realizar la carga.</li>
                  <li><strong>Rechazado: </strong>Estatus que describe el rechazo del autorizador, no se consideraran en la integración con ICM.</li>
                  <li><strong>Autorizado: </strong>Estatus que describe la aprobación de la carga por parte del autorizador, se consideraran en la integración con ICM.</li>   
            </ul>

             <p>  
                 Para autorizar alguna de las cargas en <strong>PROCESO</strong>, se tendra que hacer click en el boton de la columna "Detalle" correspondiente a cada carga. 
                 Nos mostrara la siguiente pantalla:
             </p>

             <img class="mb-3 img-fluid" src="../images/Modulos/BonosTransportes/pantalla_BonosTransporte_Autorizacion_Modal.png?v=1"/>

              <p>  Los filtros de Division y Periodo estan deshabilitados, solo son informativos. Contienen la siguiente información:</p> 
              <ul>
                   <li><strong>Division:</strong>División a la que pertenece el usuario que dio de alta el lote.</li>
                   <li><strong>Periodo:</strong>Periodo a la que pertenece el lote de carga.</li>
              </ul>
              <p> Ademas, se muestra el detalle de la carga con sus respectivos estatus, solo se muestran los que tienen estatus válidos y con avisos.  
                  Ademas al final se tiene la opción de agregar algún comentario adicional por parte del Autorizador. </p> 

              <p>En la parte inferior de la pantalla, se tienen los siguientes botones:"</p>
              <ul>
                  <li><strong>Rechazar:</strong>Actualizara el estatus de la carga a “RECHAZADO“ y enviara un correo electrónico al solicitante confirmando que el lote fue procesado.</li>
                  <li><strong>Autorizar:</strong>Actualizara el estatus de la carga a “AUTORIZADO” para que sea enviada a ICM, Ademas enviara un correo electrónico al solicitante confirmando que el lote fue procesado.</li>
             </ul>
            
            <h5 id="item-5-2">Consultar Carga Cerrada</h5>     
            <p>Para consultar alguna de las cargas <strong>CERRADAS</strong>, se tendra que hacer click en el boton de la columna <strong>Detalle</strong> correspondiente a cada carga. </p>

            <p>Nos abrira la siguiente pantalla a modo de consulta (NO EDITABLE):</p>
            <img class="mb-3 img-fluid" src="../images/Modulos/BonosTransportes/pantalla_Consultar_Autorizacion_BonosTransporte_Cerradas.png?v=1"/>


        </div>

      
    </div>
    
</div>
</asp:Content>
