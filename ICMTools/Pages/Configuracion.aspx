<%@ Page Title="" Language="vb" AutoEventWireup="false" 
    MasterPageFile="~/Master/MasterPage.Master" CodeBehind="Configuracion.aspx.vb" 
    Inherits="ICMTools.BonosTransporteConfiguracion" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>

 
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta name="author" content="Equipo SOINF"/>

    <script type="text/javascript"  >


        $(function () {
            $('[data-toggle="tooltip"]').tooltip();
        });


        var myWebServiceURL = "<%= Page.ResolveClientUrl("~/WebServices/WebServiceParametros.asmx")%>";
       
        Configuracion = {};
        Configuracion.User = '<%= If(Session("User") IsNot Nothing, CType(Session("User"), ICMTools.User).Email, "")  %>',
        Configuracion.dt = {
            Parametros: undefined,
            Reemplazos: undefined,
            MediaAnual: undefined
        }

        Configuracion.item = {
            SelectFiltroPosiciones: undefined,
            SelectFiltroPosicionesModal:undefined,
            SelectPosicionesReemplazo: undefined,
            SelectFiltroSociedadMediaAnual: undefined,
            SelectFiltroDivisionMediaAnual: undefined,                     
            SelectFiltroModulosConfiguracion: undefined,
            SelectFiltroParametrosConfiguracion: undefined
        }

        Configuracion.TipoAccion = "";
        
        window.onload = () =>
        {         
            Configuracion.getDataInit();
         
            $('#cboTipoReemplazo').on('change', function () {
                const valor = $(this).val();
                const habilitar = valor === "1";

                $('#dtePeriodo').prop('disabled', !habilitar);
                $('#dteHasta').prop('disabled', !habilitar);

                if (!habilitar) {
                    $('#dtePeriodo').val('');
                    $('#dteHasta').val('');
                }
            });
            
            $('#btnMostrarMediaAnual').click(function () {
                let Sociedad = $('#SelectFiltroSociedadMediaAnual').val();
                let Division = $('#SelectFiltroDivisionMediaAnual').val();
                let mensaje = $("#mensajeErrorMediaAnual");

                if (!Sociedad) {
                    mensaje.text("La Sociedad es obligatoria.");
                    return;
                }

                if (!Division) {
                    mensaje.text("La División es obligatoria.");
                    return;
                }

                mensaje.text("");                
                Configuracion.getDataTablaConfiguraciones_Ajustado(Sociedad, Division);

            });

            $('#btnGuardarReemplazos').click(function () {
                let TipoReemplazo = $('#cboTipoReemplazo').val();
                let UsuarioReemplazo = $('#SelectPosicionesReemplazo').val();
                let fechaInicio = $('#dtePeriodo').val();
                let fechaFin = $('#dteHasta').val();
                let mensaje = $('#mensajeReemplazos');
                let posicionID = $('#formModalParameters').data('PosicionID');

                mensaje.text('');

                if (!TipoReemplazo && !UsuarioReemplazo && !fechaInicio && !fechaFin) {
                    mensaje.text("Todos los campos son obligatorios.");
                    return;
                }

                if (!TipoReemplazo) {
                    mensaje.text("Debe seleccionar un Tipo de Reemplazo.");
                    return;
                }
                if (!UsuarioReemplazo) {
                    mensaje.text("Debe seleccionar un Usuario de Reemplazo.");
                    return;
                }


                if (TipoReemplazo === "1") {
                    if (!fechaInicio || !fechaFin) {
                        mensaje.text("Debe capturar ambas fechas.");
                        return;
                    }

                    const fechaInicioValida = !isNaN(Date.parse(fechaInicio));
                    const fechaFinValida = !isNaN(Date.parse(fechaFin));

                    if (!fechaInicioValida || !fechaFinValida) {
                        mensaje.text("Una o ambas fechas no son válidas.");
                        return;
                    }

                    if (new Date(fechaInicio) > new Date(fechaFin)) {
                        mensaje.text("La fecha de inicio no puede ser mayor que la fecha fin.");
                        return;
                    }
                }                      
                Configuracion.SaveChangesReemplazos_Ajustado(posicionID, TipoReemplazo, UsuarioReemplazo, fechaInicio, fechaFin);                
            });

            $('#btnGuardarParametros').click(function () {
                let IDParametro = $('#txtID').val();
                let Valor = $('#txtValor').val();
                let mensaje = $('#mensajeParametros');
                let TipoParametro = $('#cboTipoParametro').val();

                mensaje.text('');

                if (!IDParametro && !Valor && !TipoParametro)  {
                    mensaje.text("Todos los campos son obligatorios.");
                    return;
                }

                if (!IDParametro) {
                    mensaje.text("El Parametro es Requerido");
                    return;
                }
                if (!TipoParametro) {
                    mensaje.text("El Tipo del Parámetro es Requerido");
                    return;
                }

                if (!Valor) {
                    mensaje.text("El valor es obligatorio.");
                    return;
                }

                Configuracion.SaveChangesParametros_Ajustado(IDParametro, TipoParametro, Valor);

            });

            $('#btnGuardarMediaAnual').click(function () {
                const esEdicion = Configuracion.TipoAccion === "E";

                let ConfigurationID = $('#formModalMediaAnual').data('IDConfiguration');


                let SociedadID = $('#SelectFiltroSociedadMediaAnual').val();
                let DivisionID = $('#SelectFiltroDivisionMediaAnual').val();

                let ModuloID = undefined;
                let ParametroID = undefined;


                if (esEdicion) {
                    ModuloID = $('#formModalMediaAnual').data('IDModule');
                    ParametroID = $('#formModalMediaAnual').data('IDParameter');
                } else {
                    ModuloID = $('#SelectConfiguracionModulo').val();
                    ParametroID = $('#SelectConfiguracionParametro').val();
                }




                let Valor = $('#txtMediaAnualValor').val();
                let mensaje = $('#mensajeModalMediaAnual');

                let fechaInicio = $('#dteConfiguracionDesde').val();
                let fechaFin = $('#dteConfiguracionHasta').val();
                let ActivoActual = $('#formModalMediaAnual').data('IsActive');
                let ActivoModificado = $('#chkActivo').is(':checked');


                mensaje.text('');



                if (!esEdicion) { 
                    if (!ModuloID) {
                        mensaje.text("Debe seleccionar un Módulo.");
                        return;
                    }

                    if (!ParametroID) {
                        mensaje.text("Debe seleccionar un Parámetro.");
                        return;
                    }
                }



                if (!Valor) {
                    mensaje.text("El valor es obligatorio.");
                    return;
                }

                if ($.trim(Valor) === "") {
                    mensaje.text("El valor no puede estar vacío.");
                    return;

                } else if (!$.isNumeric(Valor)) {
                    mensaje.text("El valor debe ser numérico.");
                    return;
                }

                if (!fechaInicio || !fechaFin) {
                    mensaje.text("Debe capturar ambas fechas.");
                    return;
                }

                const fechaInicioValida = !isNaN(Date.parse(fechaInicio));
                const fechaFinValida = !isNaN(Date.parse(fechaFin));

                if (!fechaInicioValida || !fechaFinValida) {
                    mensaje.text("Una o ambas fechas no son válidas.");
                    return;
                }

                if (new Date(fechaInicio) > new Date(fechaFin)) {
                    mensaje.text("La fecha de inicio no puede ser mayor que la fecha fin.");
                    return;
                }

                if (esEdicion  ) {
                    if (ActivoActual != ActivoModificado) {
                        Swal.fire({
                            title: 'Confirmación de cambio de estatus',
                            text: "¿Estás seguro de actualizar el estado?",
                            icon: 'warning',
                            showCancelButton: true,
                            confirmButtonText: 'Continuar',
                            cancelButtonText: 'Cancelar'
                        }).then((result) => {
                            if (!result.isConfirmed) {                                
                                return;
                            } else {                                
                                Configuracion.SaveChangesConfiguracionParametros_Ajustado(ConfigurationID, SociedadID, DivisionID, Valor, fechaInicio, fechaFin, ActivoModificado);                                
                            }
                        });

                    }
                    else {                        
                        Configuracion.SaveChangesConfiguracionParametros_Ajustado(ConfigurationID, SociedadID, DivisionID, Valor, fechaInicio, fechaFin, ActivoModificado);                        
                    }

                } else {
                                       
                        Configuracion.SaveChangesNewConfiguracionParametro(SociedadID, DivisionID, ModuloID, ParametroID, Valor, fechaInicio, fechaFin, ActivoModificado)
                    
                }

            });

            $("#SelectFiltroPosiciones").change(function () {
                var valor = $(this).val();
                let mensaje = $("#mensajeErrorReemplazos");
                mensaje.text("");
                Configuracion.ChangeFiltroPosiciones(valor);               
            });

            $("#SelectFiltroPosicionesModal").change(function () {
                var valor = $(this).val();
                let mensaje = $("#mensajeReemplazos");
                let posicionID = $('#formModalParameters').data('PosicionID');
                mensaje.text("");

                if (valor) {                    
                    Configuracion.getDataPosicionesReemplazosModal_Ajustado(valor, posicionID);

                }
              
            });
                 
            $("#btnMostrarAgregarConfiguration").click(function () {

                let Sociedad = $('#SelectFiltroSociedadMediaAnual').val();
                let Division = $('#SelectFiltroDivisionMediaAnual').val();
                let mensaje = $("#mensajeErrorMediaAnual");

                if (!Sociedad)
                {
                    mensaje.text("La Sociedad es obligatoria.");
                    return;
                }

                if (!Division) {
                    mensaje.text("La División es obligatoria.");
                    return;
                }
                  


                Configuracion.TipoAccion = "A" 
                Configuracion.LimpiarControlesConfiguracionesModal();

                Configuracion.getDataTablaModulosCombo_Ajustado();                
                Configuracion.MostrarModalMediaAnual(); 
            });
                        
            $("#SelectConfiguracionModulo").change(function () {
                var valor = $(this).val();
                let mensaje = $("#mensajeModalMediaAnual");
                let Sociedad = $('#SelectFiltroSociedadMediaAnual').val();
                let Division = $('#SelectFiltroDivisionMediaAnual').val();


                mensaje.text("");

                if (valor) {
                    Configuracion.getDataTablaParametrosCombo_Ajustado(Sociedad, Division, valor);                    
                }

            });

            $("#SelectFiltroSociedadMediaAnual").change(function () {
                var sociedad = $(this).val();
                let mensaje = $("#mensajeErrorMediaAnual");
                mensaje.text("");

                if (sociedad) {
                    Configuracion.getDataTablaDivisionesSociedad_Ajustado(sociedad);
                }


                Configuracion.LimpiarTablaConfiguraciones();
              
            });

            $("#SelectFiltroDivisionMediaAnual").change(function () {
                Configuracion.LimpiarTablaConfiguraciones();
            });

        };
        
        Configuracion.getDataInit = async () => {
            Configuracion.item = {
                SelectFiltroPosiciones: document.getElementById("SelectFiltroPosiciones"),
                SelectFiltroPosicionesModal: document.getElementById("SelectFiltroPosicionesModal"),
                SelectPosicionesReemplazo: document.getElementById("SelectPosicionesReemplazo"),
                SelectFiltroSociedadMediaAnual: document.getElementById("SelectFiltroSociedadMediaAnual"),
                SelectFiltroDivisionMediaAnual: document.getElementById("SelectFiltroDivisionMediaAnual"),
                SelectFiltroModulosConfiguracion: document.getElementById("SelectConfiguracionModulo"),
                SelectFiltroParametrosConfiguracion: document.getElementById("SelectConfiguracionParametro")

            }             
            icmTools.showLoading("Cargando Configuraciones");            
            Configuracion.getDataTablaParametros_Ajustado();            
            Configuracion.getDataTablaReemplazos_Ajustado("-1");           
            Configuracion.getDataPosicionesReemplazos_Ajustado();           
            Configuracion.getDataTablaConfiguraciones_Ajustado("", "");                        
            Configuracion.getDataTablaSociedades_Ajustado();

            await delay(2000);
            icmTools.hideLoading();
        }

        Configuracion.initPosiciones = (data) => {
            config = {
                data: data,
                columns: [             
                    { data: 'ReplacementPosition', title: 'Descripción' }                  
                ]
            }            
        }
        
        Configuracion.initParametros = (data) => {
          
           config = {
                data: data,
                columns: [
                    { data: 'ParameterID', title: 'ID' },
                    { data: 'ParameterIDKey', title: 'Clave' },
                    { data: 'ParameterModule', title: 'Modulo' },
                    { data: 'ParameterName', title: 'Parametro' },
                    { data: 'ParameterValue', title: 'Valor' },
                    { data: 'ParameterTypeName', title: 'Tipo' },
                    { data: 'ParameterlastUpdate', title: 'Fecha Ultimo Cambio' },
                    { data: 'ParameterUserUpdate', title: 'Usuario Ultimo Cambio' },
                    {
                        data: null, className: "text-center", title: 'Evento',
                         defaultContent: '<i id="MessageIcon" class="fa fa-list-alt text-primary" ></i>', targets: -1
                    }

                ]
            }

            Configuracion.dt.Parametros = icmTools.Datatable("tableParameters", config);           
            Configuracion.dt.Parametros.off('click', 'i').on('click', 'i', function (e) {
                let data = Configuracion.dt.Parametros.row(e.target.closest('tr')).data();
                $('#txtID').val(data.ParameterIDKey);
                $('#txtParametro').val(data.ParameterName);
                $('#txtModulo').val(data.ParameterModule);
                $('#txtValor').val(data.ParameterValue);
                $('#cboTipoParametro').val(data.ParameterType).selectpicker('refresh');
               
                Configuracion.MostrarModalParametros();

            });           
        }   
        Configuracion.initReemplazos = (data) => {

            config = {
                data: data,
                columns: [
                    { data: 'ReplacementIDPosition', title: 'Posición ID' },
                    { data: 'ReplacementPosition', title: 'Posición' },
                    { data: 'ReplacementPayeeName', title: 'Empleado' },
                    { data: 'ReplacementSocietyName', title: 'Sociedad' },
                    { data: 'ReplacementPersonlDivisionName', title: 'División de Personal' },
                    { data: 'ReplacementActiveReplacement', title: 'Reemplazo Activo' },
                    { data: null, className: "text-center", title: 'Evento', defaultContent: '<i id="MessageIcon" class="fa fa-list-alt text-primary" ></i>', targets: -1 }

                ]
            }

            Configuracion.dt.Reemplazos = icmTools.Datatable("tableReemplazos", config);

            Configuracion.dt.Reemplazos.off('click', 'i').on('click', 'i', function (e) {
                let data = Configuracion.dt.Reemplazos.row(e.target.closest('tr')).data();
                $('#txtEmpleado').val(data.ReplacementPayeeName);


                $('#formModalParameters').data('PosicionID', data.ReplacementIDPosition);  
                $('#formModalParameters').data('Posicion', data.ReplacementPosition);

                
                Configuracion.MostrarModalReemplazos();

            });
           
            
        }   
        Configuracion.initMediasAnuales = (data) => {

            config = {
                data: data,
                columns: [
                    { data: 'MediaAnualDivisionID', title: 'División' },                   
                    { data: 'MediaAnualModuloName', title: 'Módulo' },
                    { data: 'MediaAnualParametroName', title: 'Parametro' },
                    { data: 'MediaAnualValor', title: 'Valor' },
                    { data: 'MediaAnualDesde', title: 'Desde:' },
                    { data: 'MediaAnualHasta', title: 'Hasta' },
                    { data: 'MediaAnualFechaUltimoCambio', title: 'Fecha Ultimo Cambio' },
                    { data: 'MediaAnualUsuarioUltimoCambio', title: 'Usuario Ultimo Cambio' },
                    {
                        data: 'MediaAnualActivoDescripcion', title: 'Estatus'                        
                    },

                    { data: null, className: "text-center", title: 'Evento', defaultContent: '<i id="MessageIcon" class="fa fa-list-alt text-primary" ></i>', targets: -1 }

                ]
            }

            Configuracion.dt.MediaAnual = icmTools.Datatable("tableMediaAnual", config);


            Configuracion.dt.MediaAnual.off('click', 'i').on('click', 'i', function (e) {
                let data = Configuracion.dt.MediaAnual.row(e.target.closest('tr')).data();
                $('#txtMediaAnualParametro').val(data.MediaAnualParametroName);
                $('#txtMediaAnualModulo').val(data.MediaAnualModuloName);
                $('#txtMediaAnualValor').val(data.MediaAnualValor);               
                $('#chkActivo').prop('checked', data.MediaAnualActivo);
            
                Configuracion.formatearFechaInputDate(data.MediaAnualDesde, 'dteConfiguracionDesde', 'mensajeModalMediaAnual');
                Configuracion.formatearFechaInputDate(data.MediaAnualHasta, 'dteConfiguracionHasta','mensajeModalMediaAnual');
              
                $('#formModalMediaAnual').data('IDConfiguration', data.MediaAnualID); 
                $('#formModalMediaAnual').data('IDSociety', data.MediaAnualSociedadID);   
                $('#formModalMediaAnual').data('IDPersonalDivision', data.MediaAnualDivisionID);  
                $('#formModalMediaAnual').data('IsActive', data.MediaAnualActivo);  

                $('#formModalMediaAnual').data('IDModule', data.MediaAnualModuloID);  
                $('#formModalMediaAnual').data('IDParameter', data.MediaAnualParametroID);  

                Configuracion.TipoAccion = "E";
                

                Configuracion.MostrarModalMediaAnual();
            });


        }  

        Configuracion.initSociedadesCombo = (data) => {
            config = {
                data: data,
                columns: [
                    { data: 'SocietyValue', title: 'ID' },
                    { data: 'SocietyName', title: 'Sociedad' },
                ]
            }
        }
        Configuracion.initDivisionesSociedadCombo = (data) => {
            config = {
                data: data,
                columns: [
                    { data: 'PersonnelDivisionValue', title: 'ID' },
                    { data: 'PersonnelDivisionName', title: 'División' },
                ]
            }
        }    
        Configuracion.initModulosCombo = (data) => {
            config = {
                data: data,
                columns: [
                    { data: 'ModuleIDModule', title: 'Modulo' },
                    { data: 'ModuleIDKey', title: 'Descripción' },
                ]
            }
        }
        Configuracion.initParametrosCombo = (data) => {
            config = {
                data: data,
                columns: [
                    { data: 'ParameterID', title: 'Parametro' },
                    { data: 'ParameterIDKey', title: 'Descripción' },
                ]
            }
        }        

        Configuracion.getDataPosicionesReemplazos_Ajustado = () => {            
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/ObtenerPosicionesReemplazos_Ajustado",
                data: "{}",
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {
                        console.log("Respuesta recibida Posiciones:");

                        var res = data.d;
                        if (res.Exito) {
                            if (data && data.d) {
                                console.log("Datos del Posiciones:", res.Datos);
                                if (Array.isArray(res.Datos)) {
                                    Configuracion.initPosiciones(res.Datos);

                                    var opciones = res.Datos;

                                    var SelectFiltroPosiciones = Configuracion.item.SelectFiltroPosiciones;
                                    var SelectFiltroPosicionesModal = Configuracion.item.SelectFiltroPosicionesModal;

                                    SelectFiltroPosiciones.innerHTML = "";
                                    SelectFiltroPosicionesModal.innerHTML = "";

                                    $.each(opciones, function (i, item) {
                                        var option = document.createElement("option")
                                        option.value = item.ReplacementPosition
                                        option.text = item.ReplacementPosition
                                        option.setAttribute("data-icon", "fa fa-user");
                                        SelectFiltroPosiciones.append(option)

                                    });

                                    var $ddl = $('#SelectFiltroPosiciones');
                                    $ddl.selectpicker("refresh");
                                    
                                    $.each(opciones, function (i, item) {
                                        var option = document.createElement("option")
                                        option.value = item.ReplacementPosition
                                        option.text = item.ReplacementPosition
                                        option.setAttribute("data-icon", "fa fa-user");

                                        SelectFiltroPosicionesModal.append(option)
                                    });


                                    var $ddlModal = $("#SelectFiltroPosicionesModal");
                                    $ddlModal.selectpicker("refresh");

                                }
                            } else {
                                console.log("Respuesta Posiciones del Filtro es vacía o no existe:", res.Datos);
                            }
                        } else {
                            console.log(res.Mensaje);
                        }

                       
                       

                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    $("#mensajeErrorReemplazos").text("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        Configuracion.getDataTablaParametros_Ajustado = async () => {
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/SelectParameters_Ajustado",
                data: "{}",
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {
                        console.log("Respuesta recibida:");

                        var res = data.d;
                        if (res.Exito) {
                            if (data && data.d) {
                                console.log("Datos del DataTable:", res);
                                if (Array.isArray(res.Datos)) {
                                    Configuracion.initParametros(res.Datos);
                                }
                            } else {
                                console.log("Respuesta data.d vacía o no existe:", data);
                            }
                        } else {
                            console.log(res.Mensaje);
                        }

                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    $("#mensajeErrorParametros").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        Configuracion.getDataTablaReemplazos_Ajustado = (filtro) => {       
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/SelectReplacements_Ajustado",
                data: JSON.stringify({
                    FiltroPosiciones: filtro,
                    IDPosicionExcluir: ''
                }),
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {
                        var res = data.d;
                        if (res.Exito) {
                            console.log("Respuesta recibida Reemplazos:");
                            if (data && data.d) {
                                console.log("Datos del DataTable Reemplazos:", res);
                                if (Array.isArray(res.Datos)) {
                                    Configuracion.initReemplazos(res.Datos);
                                }
                            } else {
                                console.log("Respuesta data.d vacía o no existe:", data);
                            }
                           
                        }
                        else {
                            console.log(res.Mensaje);
                        }


                        icmTools.hideLoading();

                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor:", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                     $("#mensajeErrorReemplazos").text("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        Configuracion.getDataTablaConfiguraciones_Ajustado = (Sociedad, Division) => {
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/ObtenerConfiguracionesParametros_Ajustado",
                data: JSON.stringify({
                    Sociedad: Sociedad,
                    Division: Division
                }),
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {
                        var res = data.d;

                        console.log("Respuesta recibida ConfiguracionesParametros:");

                        if (res.Exito) {

                            if (data && data.d) {
                                console.log("Datos del DataTable ConfiguracionesParametros:", res.Datos);
                                if (Array.isArray(res.Datos)) {

                                    Configuracion.initMediasAnuales(res.Datos);
                                }
                            } else {
                                console.log("Respuesta data.d vacía o no existe:", res.Datos);
                            }


                        } else {
                            console.log(res.Mensaje);
                        }

                     
                        icmTools.hideLoading();

                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor:", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    $("#mensajeErrorMediaAnual").text("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        Configuracion.getDataPosicionesReemplazosModal_Ajustado = (filtro, IDPosicionExcluir) => {
            icmTools.showLoading("Cargando Posiciones para Reemplazo")

            let mensaje = $("#mensajeReemplazos");
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/SelectReplacements_Ajustado",   
                data: JSON.stringify({
                    FiltroPosiciones: filtro,
                    IDPosicionExcluir: IDPosicionExcluir
                }),
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {
                      var res = data.d;

                        if (res.Exito)
                        {              
                          console.log("Respuesta recibida Posiciones Modal:");
                            if (data && data.d) {
                            console.log("Datos del Posiciones Modal:", res.Datos);
                            if (Array.isArray(res.Datos)) {
                                Configuracion.initPosiciones(res.Datos);

                                var opciones = res.Datos;

                                var SelectPosicionesReemplazo = Configuracion.item.SelectPosicionesReemplazo;
                                SelectPosicionesReemplazo.innerHTML = "";

                                $.each(opciones, function (i, item) {
                                    var option = document.createElement("option");
                                    option.value = item.ReplacementIDPosition;
                                    option.text = item.ReplacementPosition + '-' + item.ReplacementPayeeName;
                                    option.setAttribute("data-icon", "fa fa-user");
                                    SelectPosicionesReemplazo.append(option)
                                });

                                var $ddl = $('#SelectPosicionesReemplazo');
                                $ddl.selectpicker("refresh");


                            }
                        } else
                            {
                            console.log("Respuesta Posiciones Modal es vacía o no existe:", data);
                            }

                      }
                        icmTools.hideLoading()
                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario

                    mensaje.text("Error: " + XMLHttpRequest.responseText);

                }


            });
        }

        Configuracion.getDataTablaSociedades_Ajustado = async () => {
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/SelectSociedadesCombo_Ajustado",
                data: JSON.stringify({
                    User: Configuracion.User
                }),
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {
                        console.log("Respuesta recibida:");
                        var res = data.d;

                        if (res.Exito) {

                            if (data && data.d) {
                                console.log("Datos del DataTable:", res.Datos);
                                if (Array.isArray(res.Datos)) {

                                    Configuracion.initSociedadesCombo(res.Datos);

                                    var opciones = res.Datos;

                                    var SelectFiltroSociedad = Configuracion.item.SelectFiltroSociedadMediaAnual;

                                    SelectFiltroSociedad.innerHTML = "";

                                    $.each(opciones, function (i, item) {
                                        var option = document.createElement("option")
                                        option.value = item.SocietyValue
                                        option.text = item.SocietyName
                                        option.setAttribute("data-icon", "fas fa-users");
                                        SelectFiltroSociedad.append(option)

                                    });

                                    var $ddl = $('#SelectFiltroSociedadMediaAnual');
                                    $ddl.selectpicker("refresh");
                                }
                            } else {
                                console.log("Respuesta data.d vacía o no existe:", data);
                            }

                        } else {
                            console.log(res.Mensaje);
                        }

                      
                       

                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    $("#mensajeErrorParametros").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }


        Configuracion.getDataTablaDivisionesSociedad_Ajustado = (SociedadID) => {
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/SelectDivisionesSociedadCombo_Ajustado",
                data: JSON.stringify({
                   
                    SociedadID: SociedadID
                }),
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {

                        console.log("Respuesta recibida:");
                        var res = data.d;

                        if (res.Exito) {

                            if (data && data.d) {
                                console.log("Datos del DataTable:", res.Datos);
                                if (Array.isArray(res.Datos)) {

                                    Configuracion.initDivisionesSociedadCombo(res.Datos);

                                    var opciones = res.Datos;

                                    var SelectFiltroDivisionMediaAnual = Configuracion.item.SelectFiltroDivisionMediaAnual;

                                    SelectFiltroDivisionMediaAnual.innerHTML = "";

                                    $.each(opciones, function (i, item) {
                                        var option = document.createElement("option")
                                        option.value = item.PersonnelDivisionValue
                                        option.text = item.PersonnelDivisionName
                                        option.setAttribute("data-icon", "fas fa-users");
                                        SelectFiltroDivisionMediaAnual.append(option)

                                    });

                                    var $ddl = $('#SelectFiltroDivisionMediaAnual');
                                    $ddl.selectpicker("refresh");
                                }
                            } else {
                                console.log("Respuesta data.d vacía o no existe:", res.Datos);
                            }

                        }
                        else {
                            console.log(res.Mensaje);
                        }
                        

                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    $("#mensajeErrorParametros").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        Configuracion.getDataTablaModulosCombo_Ajustado =  () => {
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/SelectModulesCombo_Ajustado",
                data: "{}",
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {

                        console.log("Respuesta recibida:");
                        var res = data.d;

                        if (res.Exito) {

                            if (data && data.d) {
                                console.log("Datos del DataTable:", res.Datos);
                                if (Array.isArray(res.Datos)) {
                                    Configuracion.initModulosCombo(res.Datos);

                                    var opciones = res.Datos;

                                    var SelectFiltroModulos = Configuracion.item.SelectFiltroModulosConfiguracion;

                                    SelectFiltroModulos.innerHTML = "";

                                    $.each(opciones, function (i, item) {
                                        var option = document.createElement("option")
                                        option.value = item.ModuleIDModule
                                        option.text = item.ModuleIDKey
                                        option.setAttribute("data-icon", "fas fa-puzzle-piece");
                                        SelectFiltroModulos.append(option)

                                    });

                                    var $ddl = $('#SelectConfiguracionModulo');
                                    $ddl.selectpicker("refresh");
                                }
                            } else {
                                console.log("Respuesta data.d vacía o no existe:", res);
                            }


                        } else {
                            console.log(res.Mensaje);
                        }

                       

                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    $("#mensajeErrorParametros").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        Configuracion.getDataTablaParametrosCombo_Ajustado = async (SociedadID, DivisionID, ModuloID) => {
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/SelectParametersCombo_Ajustado",
                data: JSON.stringify({
                    SociedadID: SociedadID,
                    DivisionID: DivisionID,
                    ModuloID: ModuloID
                    
                }),
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success:
                    function (data) {

                        console.log("Respuesta recibida:");
                        var res = data.d;

                        if (res.Exito) {
                            if (data && data.d) {
                                console.log("Datos del DataTable:", res.Datos);
                                if (Array.isArray(res.Datos)) {
                                    Configuracion.initParametrosCombo(res.Datos);

                                    var opciones = res.Datos;

                                    var SelectFiltroParametrosConfiguracion = Configuracion.item.SelectFiltroParametrosConfiguracion;

                                    SelectFiltroParametrosConfiguracion.innerHTML = "";

                                    $.each(opciones, function (i, item) {
                                        var option = document.createElement("option")
                                        option.value = item.ParameterID
                                        option.text = item.ParameterIDKey
                                        option.setAttribute("data-icon", "fas fa-clipboard-check");
                                        SelectFiltroParametrosConfiguracion.append(option)

                                    });

                                    var $ddl = $('#SelectConfiguracionParametro');
                                    $ddl.selectpicker("refresh");
                                }
                            } else {
                                console.log("Respuesta data.d vacía o no existe:", res);
                            }



                        } else {
                            console.log(res.Mensaje);
                        }


                       
                    },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    $("#mensajeErrorParametros").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        Configuracion.SaveChangesParametros_Ajustado = async (IDParametro, TipoParametro, Valor) => {
            icmTools.showLoading("Guardando Parametos");
            await delay(1000);
            let mensaje = $('#mensajeParametros');
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/GuardarDatosParametros",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    IDKey: IDParametro,
                    Valor: Valor,
                    TipoParametro: TipoParametro,
                    User: Configuracion.User
                }),
                dataType: "json",
                success:  function (data) {
                   
                    icmTools.hideLoading();

                    var res = data.d;
                    if (res.Exito) {
                        console.log(res.Mensaje);

                        $('#formModalParameters').modal('hide');

                        $('#txtID').val('');
                        $('#txtParametro').val('');
                        $('#txtModulo').val('');
                        $('#txtValor').val('');


                        icmTools.showLoading("Cargando Parametros");                       
                        Configuracion.getDataTablaParametros_Ajustado();
                        icmTools.hideLoading();
                    } else {
                        console.error(res.Mensaje);
                        mensaje.text(res.Mensaje);
                    }
                   
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading();
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText);
                    mensaje.text("Error al guardar los datos de Parametros.");
                }
            });
    
        }

        Configuracion.SaveChangesReemplazos_Ajustado = async (PosicionID, TipoReemplazo, UsuarioReemplazo, Periodo, Hasta) => {
            icmTools.showLoading("Guardando Reemplazos");
            await delay(1000);
            let mensaje = $('#mensajeReemplazos');
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/GuardarDatosReemplazos_Ajustado",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    PosicionID: PosicionID,
                    TipoReemplazo: TipoReemplazo,
                    UsuarioReemplazo: UsuarioReemplazo,
                    Periodo: Periodo,
                    Hasta: Hasta,
                    User: Configuracion.User
                }),
                dataType: "json",
                success: function (data) {

                    icmTools.hideLoading();
                    var res = data.d;
                    if (res.Exito) {
                        $('#formModalReplacement').modal('hide');
                        Configuracion.LimpiarControlesReemplazoModal();


                        let ComboPosicion = $('#SelectFiltroPosiciones').val()
                        icmTools.showLoading("Cargando Posiciones");
                        //Configuracion.getDataTablaReemplazos(ComboPosicion);
                        Configuracion.getDataTablaReemplazos_Ajustado(ComboPosicion);
                        icmTools.hideLoading();
                    }
                    else {
                        console.error(res.Mensaje);
                        mensaje.text(res.Mensaje);
                    }   
                  
                },
                error: function () {
                    mensaje.text("Error al guardar los datos de los Reemplazos.");
                    icmTools.hideLoading();
                }
            });
        }

        Configuracion.SaveChangesConfiguracionParametros_Ajustado = async (IDConfiguration, SociedadID, DivisionID, Valor, StartDate, EndDate, IsActive) => {
            icmTools.showLoading("Guardando Configuración");
            await delay(1000);
            let mensaje = $('#mensajeModalMediaAnual'); 
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/GuardarDatosConfiguracionesParametros_Ajustado",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    IDConfiguration: IDConfiguration,
                    Valor: Valor,
                    StartDate: StartDate,
                    EndDate: EndDate,
                    IsActive: IsActive,
                    User: Configuracion.User
                }),
                dataType: "json",
                success: function (data) {
                    var res = data.d;
                    icmTools.hideLoading();
                    if (res.Exito) {

                        $('#formModalMediaAnual').modal('hide');

                        Configuracion.LimpiarControlesConfiguracionesModal();

                        icmTools.showLoading("Cargando Configuraciones");
                        //Configuracion.getDataTablaConfiguraciones(SociedadID, DivisionID);
                        Configuracion.getDataTablaConfiguraciones_Ajustado(SociedadID, DivisionID);
                        icmTools.hideLoading();

                    } else {
                        console.error(res.Mensaje);
                        mensaje.text(res.Mensaje);
                    }

                    
                   
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading();
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText);
                    mensaje.text("Error al guardar los datos de Configuración.");
                }
            });

        }

        Configuracion.SaveChangesNewConfiguracionParametro = async (SociedadID, DivisionID, ModuloID, ParametroID, Valor, StartDate, EndDate, IsActive) => {
            icmTools.showLoading("Guardando Nueva Configuración");
            await delay(1000);                    

            let mensaje = $('#mensajeModalMediaAnual');
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/GuardarDatosNuevaConfiguracionParametro_Ajustado",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    SociedadID: SociedadID,
                    DivisionID: DivisionID,
                    ModuloID: ModuloID,
                    ParametroID: ParametroID,
                    Valor: Valor,
                    StartDate: StartDate,
                    EndDate: EndDate,
                    IsActive: IsActive,
                    User: Configuracion.User
                }),
                dataType: "json",
                success: function (data) {

                    var res = data.d;

                    icmTools.hideLoading();

                    if (res.Exito) {
                        $('#formModalMediaAnual').modal('hide');

                        Configuracion.LimpiarControlesConfiguracionesModal();

                        icmTools.showLoading("Cargando Configuraciones");
                        Configuracion.getDataTablaConfiguraciones_Ajustado(SociedadID, DivisionID);
                        icmTools.hideLoading();
                    }
                    else {
                        console.error(res.Mensaje);
                        mensaje.text(res.Mensaje);
                    }
                  
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading();
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText);
                    mensaje.text("Error al guardar los datos de Configuración.");
                }
            });

        }

        Configuracion.ChangeFiltroPosiciones = async (valor) => {

            icmTools.showLoading("Cargando Posiciones");

            //Configuracion.getDataTablaReemplazos(valor);
            Configuracion.getDataTablaReemplazos_Ajustado(valor);


        }


        //Funciones Generales de la pagina
        Configuracion.LimpiarTablaConfiguraciones = () => {
            var tabla = $('#tableMediaAnual').DataTable();
            tabla.clear().draw();
        }

        Configuracion.LimpiarControlesReemplazoModal = () => {
            $('#cboTipoReemplazo').val('').selectpicker('refresh');
            $('#SelectPosicionesReemplazo').val('').selectpicker('refresh');
            $('#SelectFiltroPosicionesModal').val('').selectpicker('refresh');
            $('#dtePeriodo').val('');
            $('#dteHasta').val('');
        }

        Configuracion.LimpiarControlesConfiguracionesModal = () => {
            $('#SelectConfiguracionModulo').selectpicker('val', '').selectpicker('refresh');
            $('#SelectConfiguracionParametro').selectpicker('val', '').selectpicker('refresh');
            $('#txtMediaAnualParametro').val('');
            $('#txtMediaAnualModulo').val('');
            $('#txtMediaAnualValor').val('');
            $('#dteConfiguracionDesde').val('');
            $('#dteConfiguracionHasta').val('');
            $('#chkActivo').prop('checked', false);
        }

        Configuracion.formatearFechaInputDate = (fechaTexto, idInput, idError) => {

            // Validar formato DD-MM-YYYY
            var regexFecha = /^(\d{2})-(\d{2})-(\d{4})$/;
            var $error = $('#' + idError);
            var $input = $('#' + idInput);

            if (!fechaTexto) {

                $error.text('');
                $input.val('');
                return;
            }

            if (!regexFecha.test(fechaTexto)) {
                $error.text("Formato inválido. Usa DD-MM-YYYY.");
                $input.val('');
                return;
            }

            var partes = fechaTexto.split('-');
            var dia = parseInt(partes[0], 10);
            var mes = parseInt(partes[1], 10) - 1;
            var anio = parseInt(partes[2], 10);

            var fecha = new Date(anio, mes, dia);

            // Validar que sea una fecha real
            if (
                fecha.getFullYear() !== anio ||
                fecha.getMonth() !== mes ||
                fecha.getDate() !== dia
            ) {
                $error.text("La fecha no es válida.");
                $input.val('');
                return;
            }

            // Convertir y asignar
            var fechaFormateada = fecha.toISOString().split('T')[0];
            $input.val(fechaFormateada);
            $error.text(''); // Limpiar error
        }

        function delay(ms) {
            return new Promise(resolve => setTimeout(resolve, ms));
        }


        //Funciones Mostrar Modales de la pagina

        Configuracion.MostrarModalParametros = async () => {
            let mensaje = $('#mensajeParametros');
            mensaje.text("");
            

            $('#formModalParameters').modal('show');
        }
        Configuracion.MostrarModalReemplazos = () => {           
            let posicionID = $('#formModalParameters').data('PosicionID');
            let posicionName = $('#formModalParameters').data('Posicion');
            let mensaje = $('#mensajeErrorReemplazos');
            let mensajeModal = $('#mensajeReemplazos');
            
            mensaje.text("");
            mensajeModal.text("");

            

            if (posicionID) {
               
                Configuracion.LimpiarControlesReemplazoModal();

                $("#formTitleModalReemplazos")                  
                    .text("Posición a Reemplazar:    " + posicionID + "-" + posicionName);

                $('#formModalReplacement').modal('show')
            } else {

                mensaje.text("La Posicion a Editar no es Valida.");
            }
        }
        Configuracion.MostrarModalMediaAnual = () => {
            const esEdicion = Configuracion.TipoAccion === "E";
            let mensaje = $('#mensajeModalMediaAnual');
            mensaje.text("");

            

            if (esEdicion) {

                let IDConfiguration = $('#formModalMediaAnual').data('IDConfiguration');
                let IDSociety = $('#formModalMediaAnual').data('IDSociety');
                let IDPersonalDivision = $('#formModalMediaAnual').data('IDPersonalDivision');

                // Ocultar selects
                $('#SeccionSelectModulo').addClass('d-none');
                $('#SeccionSelectParametro').addClass('d-none');
                $('#SelectConfiguracionModulo').selectpicker('val', '').selectpicker('refresh');
                $('#SelectConfiguracionParametro').selectpicker('val', '').selectpicker('refresh');

                // Mostrar inputs
                $('#txtMediaAnualModulo').removeClass('d-none');
                $('#txtMediaAnualParametro').removeClass('d-none');

                $("#formTitleModalMediaAnual").text("Configuración a editar:  " + IDSociety + "-" + IDPersonalDivision);
            } else {
              
                let Sociedad = $('#SelectFiltroSociedadMediaAnual').val();
                let Division = $('#SelectFiltroDivisionMediaAnual').val();

                // Mostrar selects
                $('#SeccionSelectModulo').removeClass('d-none');
                $('#SeccionSelectParametro').removeClass('d-none');
                $('#SelectConfiguracionModulo').selectpicker('refresh');
                $('#SelectConfiguracionParametro').selectpicker('refresh');

                // Ocultar inputs
                $('#txtMediaAnualModulo').addClass('d-none');
                $('#txtMediaAnualParametro').addClass('d-none');

                $("#formTitleModalMediaAnual").text("Nueva Configuración en : " + Sociedad  + "-" + Division );
            }

            $('#formModalMediaAnual').modal('show');
        };
    
    </script>

</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <!-- Bootstrap JS (requiere jQuery para versiones anteriores, pero no para Bootstrap 5+) -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

  

  <%--<form id="My" runat="server">--%> 
    <div class="container-fluid Home">
        <div class="row">
            <div class="col-2">
            </div>
            <div class="col-10">                
                <!-- Nav tabs -->
                <ul class="nav nav-tabs" role="tablist">
                    <li class="nav-item">
                        <a class="nav-link active" href="#ParametrosContent" role="tab" data-toggle="tab"><i class="fas fa-cog fa-fw"></i> Parametros</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="#ReemplazosContent" role="tab" data-toggle="tab"><i class="fas fa-cog fa-fw"></i> Reemplazos</a>
                    </li>
                      <li class="nav-item">
                         <a class="nav-link" href="#MediaAnualContent" role="tab" data-toggle="tab"><i class="fas fa-cog fa-fw"></i> Configuraciones</a>
                      </li>
                </ul>

                <!-- Tab panes -->
                <div class="tab-content">
                  <div role="tabpanel" class="tab-pane active" id="ParametrosContent">
                        <div class="container-fluid">                      
                            <div class="row">
                                <table id="tableParameters" cellpadding="5" class="table table-responsive-sm table-striped table-sm">                                    
                                </table>                            
                            </div>
                            <div id="mensajeErrorParametros" class="mt-2 text-danger"></div>           
                        </div>               
                  </div>
                  <div role="tabpanel" class="tab-pane fade" id="ReemplazosContent">
                      <div class="container-fluid">

                           <div class="row mb-3">
                               <div class="col col-1">
                                     <label for="SelectFiltroPosiciones" class="form-label">Posiciones: </label>   
                               </div>
                              
                               <div class="col col-8">
                                    <select id="SelectFiltroPosiciones" data-width="100%" class="selectpicker form-select show-tick"  
                                        title="Seleccione una opción" data-live-search="true" data-size="10" ></select>                              
                               </div>                                               
                            </div>
                          <div class="row">
                             
                             <table id="tableReemplazos" style="width:100%" cellpadding="5" class="table table-responsive-sm table-striped table-sm"> 
                             </table>
                                                    
                          </div>
                            <div id="mensajeErrorReemplazos" class="mt-2 text-danger"></div>
                      </div>
                  </div>   
                  <div role="tabpanel" class="tab-pane fade" id="MediaAnualContent">
                     <div class="container-fluid">

                     <div class="row mb-3">
                         <div class="col col-1">
                               <label for="SelectFiltroSociedadMediaAnual" class="form-label m-e-0">Sociedad: </label>   
                         </div>
            
                         <div class="col col-3">
                              <select id="SelectFiltroSociedadMediaAnual" data-width="100%" class="selectpicker form-select show-tick m-0"  
                                  title="Seleccione una opción" data-live-search="true" data-size="10" ></select>                              
                         </div>  
                          <div class="col col-1">
                                <label for="SelectFiltroDivisionMediaAnual" class="form-label">División: </label>   
                          </div>
                          <div class="col col-4">
                                <select id="SelectFiltroDivisionMediaAnual" data-width="100%" class="selectpicker form-select show-tick"  
                                  title="Seleccione una opción" data-live-search="true" data-size="10" ></select>                              
                         </div> 
                         
                          <div class="col col-2">
                             <button id="btnMostrarMediaAnual" type="button" class="btn btn-primary" data-toggle="tooltip" data-placement="top" title="Obtener las Configuraciones" >Buscar</button>
                                <button id="btnMostrarAgregarConfiguration" class="btn btn-light" data-toggle="tooltip" data-placement="top" title="Nueva Configuración">
                                   <i class="fa fa-solid fa-plus"></i>
                                </button>
                          </div>
                      

                      </div>
                        <div class="row">
           
                           <table id="tableMediaAnual" style="width:100%" cellpadding="5" class="table table-responsive-sm table-striped table-sm"> 
                               <tbody></tbody>
                           </table>
                                  
                        </div>
                            <div id="mensajeErrorMediaAnual" class="mt-2 text-danger"></div>
                        </div>
                    </div>
                </div>
            </div>

        </div>
   </div>

    
      <%--modal Parametros--%>
    <div class="modal fade" id="formModalParameters" tabindex="-1" role="dialog"  aria-labelledby="formModalParametros" aria-hidden="true">
        <style>
            .modal-ku {
                width: 1250px !important;
                margin: auto;
                }
        </style>
           
        <div class="modal-dialog modal-lg modal-ku carousel-fade" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="formTitleModalParametros">Editar Parametro </h5>
                        <button type="button" class="close" data-bs-dismiss="modal" aria-label="Cerrar" data-toggle="tooltip" data-placement="top" title="Cerrar"></button>
                        <span aria-hidden="true">&times;</span>
                    </div>
                    <div class="modal-body">

                        <div class="mb-3">
                            <label for="txtID" class="form-label">ID</label>                
                            <input type="text" id="txtID" class="form-control" disabled="disabled" />
                        </div>
                        <div class="mb-3">
                            <label for="txtParametro" class="form-label">Parametro: </label>
                            <input text="text" id="txtParametro"  class="form-control" disabled="disabled" />
                          
                        </div>
                        <div class="mb-3">
                            <label for="txtModulo" class="form-label">Modulo</label>
                             <input type="text" id="txtModulo"  class="form-control" disabled="disabled"/>
                          
                        </div>
                        <div class="row">
                            <div class="col col-md-4" >
                                 <label for="cboTipoParametro" class="form-label">Tipo: </label>
                                 <select id="cboTipoParametro" name="cboTipoReemplazo" class="selectpicker form-select show-tick" title="Seleccione una opción" >                     
                                      <option value="U">Parámetro único </option>
                                      <option value="M">Parámetro x Division</option>
                                             	
                                 </select>
                            </div>
                            <div class="col col-md-5">
                                <label for="txtValor" class="form-label">Valor</label>
                                <input type="text" id="txtValor"  class="form-control" maxlength="10" />
                             </div>
                        </div>
                         
                         <div id="mensajeParametros" class="mt-2 text-danger"></div>
                    </div>
                    <div class="modal-footer">
                      
                        <button id="btnGuardarParametros" type="button" class="btn btn-primary" data-toggle="tooltip" data-placement="top" title="Actualiza el parametro">  
                            <i class="fas fa-upload"></i>
                        </button>
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" data-toggle="tooltip" data-placement="top" title="Cancelar" >
                             <i class="fas fa-window-close"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>

     <%--modal Reemplazos--%>
    <div class="modal fade" id="formModalReplacement" tabindex="-1" role="dialog"  aria-labelledby="formModalReplacement" aria-hidden="true">
    <style>
        .modal-ku {
            width: 1250px !important;
            margin: auto;
            }
    </style>
    <div class="modal-dialog modal-xl  carousel-fade" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="formTitleModalReemplazos">Posición a Reemplazar: </h5>
                    <button type="button" class="close" data-bs-dismiss="modal" aria-label="Cerrar" data-toggle="tooltip" data-placement="top" title="Cerrar"></button>
                     <span aria-hidden="true">&times;</span>
                </div>
                <div class="modal-body">
                
                    <div class="mb-3">
                        <label for="txtEmpleado" class="form-label">Empleado:</label>                
                        <input type="text" id="txtEmpleado"  class="form-control"  disabled="disabled"/>
                    </div>
                    <div class="mb-3">
                       <label for="txtParametro" class="form-label">Tipo: </label>
                       <select id="cboTipoReemplazo" name="cboTipoReemplazo" class="selectpicker form-select show-tick" title="Seleccione una opción" >                     
                            <option value="1">Remplazo Temporal </option>
                            <option value="2">Remplazo Permanente</option>
                            <option value="3">Remplazo Baja</option>
                           	
                       </select>
                      
                    </div>
                    <div class="row mb-3">
                        <div class="col col-md-3 mb-3">
                           <label for="SelectPosicionesReemplazoModal" class="form-label">Posicion:</label>
                           
                              <select id="SelectFiltroPosicionesModal" data-width="100%" class="selectpicker form-select show-tick"  
                                  title="Seleccione una opción" data-live-search="true" data-size="10" >
                                   
                              </select>                              
                          
                        </div>
                        <div class="col col-md-3 mb-3">
                              <label for="SelectPosicionesReemplazo" class="form-label">Reemplazo:</label>
                              <select id="SelectPosicionesReemplazo" class="selectpicker form-select show-tick"  title="Seleccione una opción" 
                                  data-live-search="true" data-size="6" data-width="100%"  >                                                             
                             </select>
                        </div>                       
                        <div class="col-md-3 mb-3">
                          <label for="dtePeriodo" class="form-label">Desde:</label>
                          <input type="date" id="dtePeriodo" class="form-control" disabled="disabled"/>
                        </div>  
                        <div class="col-md-3 mb-3">
                          <label for="dteHasta" class="form-label">Hasta:</label>
                          <input type="date" id="dteHasta" class="form-control" disabled="disabled"  />
                        </div>
                                    
                    </div>
                     <div id="mensajeReemplazos" class="mt-2 text-danger"></div>
                     
                </div>
                <div class="modal-footer">
                   
                    <button id="btnGuardarReemplazos" type="button" class="btn btn-primary" data-toggle="tooltip" data-placement="top" title="Guarda el Reemplazo">
                         <i class="fas fa-upload"></i>
                    </button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" data-toggle="tooltip" data-placement="top" title="Cancelar">
                         <i class="fas fa-window-close"></i>
                    </button>
                </div>
            </div>
        </div>
    </div>


       <%--modal Configuraciones --%>
    <div class="modal fade" id="formModalMediaAnual" tabindex="-1" role="dialog"  aria-labelledby="formModalMediaAnual" aria-hidden="true">
           
     <div class="modal-dialog modal-lg carousel-fade" role="document">
             <div class="modal-content">
                 <div class="modal-header">
                     <h5 class="modal-title" id="formTitleModalMediaAnual">Editar Configuración:  </h5>
                     <button type="button" class="close" data-bs-dismiss="modal" aria-label="Cerrar" data-toggle="tooltip" data-placement="top" title="Cerrar"></button>
                     <span aria-hidden="true">&times;</span>
                 </div>
                 <div class="modal-body">

                     <div class="mb-3">
                      
                         <label for="txtMediaAnualModulo" class="form-label">Módulo</label>
                         <input type="text" id="txtMediaAnualModulo"  class="form-control" disabled="disabled"/>
                        
                         <div id="SeccionSelectModulo" >                       
                             <select id="SelectConfiguracionModulo" class="selectpicker form-select show-tick"  title="Seleccione una opción" 
                                 data-live-search="true" data-size="6" data-width="100%"  >                                                             
                           </select>

                          </div>
                     </div>
                     <div class="mb-3">
                          <label for="txtMediaAnualParametro" class="form-label">Parametro: </label>
                          <input text="text" id="txtMediaAnualParametro"  class="form-control" disabled="disabled" />

                         <div id="SeccionSelectParametro" > 
                              <select id="SelectConfiguracionParametro" class="selectpicker form-select show-tick"  title="Seleccione una opción" 
                                        data-live-search="true" data-size="6" data-width="100%"  >                                                             
                              </select>
                          </div>
                     </div>
                        <div class="row mb-3">
                          <div class="col-md-4 mb-3">
                              <label for="txtMediaAnualValor" class="form-label">Valor</label>
                              <input type="text" id="txtMediaAnualValor"  class="form-control"  maxlength="10"/>
                           </div>
                          <div class="col-md-4 mb-3">
                            <label for="dteConfiguracionDesde" class="form-label">Desde:</label>
                            <input type="date" id="dteConfiguracionDesde" class="form-control"/>
                          </div>  
                          <div class="col-md-4 mb-3">
                               <label for="dteConfiguracionHasta" class="form-label">Hasta:</label>
                               <input type="date" id="dteConfiguracionHasta" class="form-control"   />
                          </div>

                        </div>
                        <div class="row mb-3">
                              
                            <div class="col col-md-2">                              
                                <label style="cursor: pointer;">
                                  <input type="checkbox" id="chkActivo" /> Activo                                 
                                </label>                            
                            </div>                           
                        </div>


                      <div id="mensajeModalMediaAnual" class="mt-2 text-danger"></div>
                 </div>
                 <div class="modal-footer">                   
                     <button id="btnGuardarMediaAnual" type="button" class="btn btn-primary" data-toggle="tooltip" data-placement="top" title="Actualiza la Configuración">                       
                         <i class="fas fa-upload"></i>
                     </button>
                     <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" data-toggle="tooltip" data-placement="top" title="Cancelar">
                         <i class="fas fa-window-close"></i>
                     </button>
                 </div>
             </div>
         </div>
   </div>

  <%--</form>--%>
</asp:Content>
