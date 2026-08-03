<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="EmpleadosActivosOracleDocumentacion.aspx.vb" Inherits="ICMTools.EmpleadosActivosOracleDocumentacion" %>
<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>

<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/EmpleadosActivosOracle.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/EmpleadosActivosOracleDocumentacion.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Form para Descargar plantilla e información de ayuda para el llenado"/>
    <meta name="author" content="Equipo SOINF"/>
    <title>Documentación de Empleados Activos Oracle</title>

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
                  <a class="nav-link active" href="#item-1">Módulo de Empleados Activos Oracle</a>
                  <nav class="nav nav-pills flex-column">
                    <a class="nav-link ml-3 my-1" href="#item-1-1">Objetivo</a>
                    <a class="nav-link ml-3 my-1" href="#item-1-2">Acceso y Uso</a>
                  </nav>
                  <a class="nav-link" href="#item-2">Plantilla</a>
                  <nav class="nav nav-pills flex-column">
                  </nav>
                  <a class="nav-link" href="#item-3">Ejecución</a>

                  </nav>
              </nav>
            </div>
            <div class="col-md-8">
                <h4 id="item-1">Módulo de Empleados Activos Oracle</h4>
                <p>Módulo de ICM Tools creado en Octubre 2025 para uso del Modelo de Varicent ICM Cloud FEMCOVS.</p>
                <h5 id="item-1-1">Objetivo</h5>
                <p>Apoyar al usuario en la tarea de ejecutar el proceso de Empleados Activos Oracle de manera sencilla y rápida.</p>
                <h5 id="item-1-2">Acceso y Uso</h5>
                <p>El acceso a este módulo es exclusivo desde el portal ICMWeb y su uso es solo para FEMCOVS.</p>
                
                <h4 id="item-2">Plantilla</h4>
                <p>La <a href="../Pages/EmpleadosActivosOracle.aspx">página de carga</a> no requiere de una plantilla, ya que no depende de un archivo para ejecutarse.</p>
               
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                  <strong>¡Importante!</strong> Esta pantalla obtiene la información directamente de la base de datos, por ende no requiere de una plantilla para su ejecución.
                  <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                  </button>
                </div>
                
                <h4 id="item-3">Ejecución</h4>
                <p>Para iniciar la ejecución de Empleados Activos Oracle deberá acceder a la <a href="../Pages/EmpleadosActivosOracle.aspx">página de carga</a>.</p>
                <img class="mb-3 img-fluid" src="../images/Modulos/EmpleadosActivosOracle/Pantalla_EmpleadosActivosOracle.png?v=2"/>
                <p>Deberá seleccionar una opción en cada uno de los filtros y elegir el archivo que desea utilizar:</p>
                <div class="alert alert-warning alert-dismissible fade show text-justify" role="alert">
                    <strong>¡Importante!</strong> En el caso de la pantalla de Empleados Activos Oracle no es necesario llenar las cajas de selección Sociedad y División de Personal.
                    <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>

                <ul>
                    <li><strong>Sociedad: </strong>Las opciones en este filtro mostrarán las sociedades a las que el usuario tiene acceso de acuerdo con la configuración dentro de ICM.</li>
                    <li><strong>División de Personal: </strong>Según la sociedad seleccionada, mostrará las divisiones de personal configuradas en las jerarquías en ICM.</li>
                </ul>

                <p>Finalmente deberá dar clic en el botón "Iniciar Ejecución de Empleados Activos Oracle" para iniciar el proceso de Empleados Activos Oracle.</p>
            </div>
        </div>
        
    </div>
</asp:Content>