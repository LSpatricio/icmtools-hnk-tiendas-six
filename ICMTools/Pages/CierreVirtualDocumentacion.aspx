<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="CierreVirtualDocumentacion.aspx.vb" Inherits="ICMTools.CierreVirtualDocumentacion" %>
<%@ MasterType VirtualPath="~/Master/MasterPage.Master" %>

<%--Contenedor de botones en TopBar--%>
<asp:Content ID="TopbarContent" ContentPlaceHolderID="TopbarContent" runat="server">
    <div class="d-flex gap-1">
        <a href="../Pages/CierreVirtual.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-upload fa-2x"></i>
            <small>Carga</small>
        </a>
        <a href="../Pages/CierreVirtualDocumentacion.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
            <i class="fas fa-book fa-2x"></i>
            <small>Documentación</small>
        </a>
    </div>
</asp:Content>

<%--Información de módulo--%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="description" content="Descripción" />
    <meta name="author" content="Autor" />
    <title>Cierre Virtual Documentación</title>
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
                <a class="nav-link active" href="#item-1">Módulo de Cierre Virtual</a>
                <nav class="nav nav-pills flex-column">
                <a class="nav-link ml-3 my-1" href="#item-1-1">Objetivo</a>
                <a class="nav-link ml-3 my-1" href="#item-1-2">Acceso y Uso</a>
                <a class="nav-link ml-3 my-1" href="#item-1-3">Cierre Virtual</a>
                <a class="nav-link ml-3 my-1" href="#item-1-4">Sección de Porcentaje</a>
                <a class="nav-link ml-3 my-1" href="#item-1-5">Sección de Tiendas por Plaza</a>
                <a class="nav-link ml-3 my-1" href="#item-1-6">Sección Documentos Oracle EBS</a>
                <a class="nav-link ml-3 my-1" href="#item-1-7">Sección de Tiendas por Distrito</a>
                </nav>
            </nav>
            </nav>
        </div>
        <div class="col-md-8">
            <%--Cierre Virtual--%>

            <h4 id="item-1">Módulo Cierre Virtual</h4>
            <p>Módulo de ICM Tools creado en Octubre de 2025 para uso del modelo de Varicent ICM Cloud FEMCO.</p>

            <h5 id="item-1-1">Objetivo</h5>
            <p>
                Reporte para visualizar el porcentaje de avance del <strong>Cierre Virtual</strong> de las tiendas del modelo <strong>FEMCO</strong>, 
                tanto a nivel general como por plaza, indicando el progreso de las tiendas que han completado el proceso.
            </p>

            <p style="font-style: italic; color: #555;">
                <strong>Nota:</strong> La sección de <em>documentos generados por plaza</em> se encuentra en proceso de definición.
            </p>

            <h5 id="item-1-2">Acceso y Uso</h5>
            <p>El acceso a este módulo es exclusivo desde el portal ICMWeb y su uso es solo para FEMCO.</p>    

            <h5 id="item-1-3">Cierre Virtual</h5>    
            <p>Muestra porcentaje de las divisiónes cerradas. (Cerradas entre las totales a nivel nacional).</p>
            <img class="mb-3 img-fluid" src="../images/Modulos/CierreVirtual/Arriba.png?v=1"/>

            <h5 id="item-1-4">Sección de Porcentaje</h5>
            <p>Porcentaje es la división de tiendas cerradas entre tiendas a nivel nacional.</p>
            <img class="mb-3 img-fluid" src="../images/Modulos/CierreVirtual/ArribaSegundo.png?v=1"/>


            <h5 id="item-1-5">Sección de Tiendas por Plaza</h5>
            <p>Representa el porcentaje de Tiendas Cerradas dividido entre Tiendas por Plaza.</p>
            <img class="mb-3 img-fluid" src="../images/Modulos/CierreVirtual/Izquierda.png?v=1"/>

            <h5 id="item-1-6">Sección Documentos Oracle EBS</h5>
            <p>Representa el porcentaje de Tiendas Cerradas dividido entre Tiendas por Plaza de documentos generados. </p>
            <p style="font-style: italic; color: #555;">
                <strong>Nota:</strong> La sección de <em>documentos generados por plaza</em> se encuentra en proceso de definición.
            </p>
            <img class="mb-3 img-fluid" src="../images/Modulos/CierreVirtual/Derecha.png?v=1"/>

            <h5 id="item-1-7">Sección de Tiendas por Distrito</h5>
            <p>El porcentaje representa la proporción de Tiendas Cerradas a nivel distrito.</p>
            <img class="mb-3 img-fluid" src="../images/Modulos/CierreVirtual/Abajo.png?v=1"/>
        </div>
    </div>
    </div>
</asp:Content>
