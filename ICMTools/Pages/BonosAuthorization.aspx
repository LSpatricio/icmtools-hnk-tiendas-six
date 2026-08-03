<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="BonosAuthorization.aspx.vb" Inherits="ICMTools.BonosAuthorization" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TopbarContent" runat="server">
   <div class="container-fluid">
<div class="d-flex gap-1">
      <a href="../Pages/BonosUpload.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
        <i class="fas fa-upload fa-2x"></i>
        <small>Carga</small>
      </a>
    
     
      <a href="../Pages/BonosAuthorization.aspx"  class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
        <i class="fas fa-check-square fa-2x"></i>
        <small>Autorización</small>
      </a>

     <a href="../Pages/BonosDocumentation.aspx" class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
   <i class="fas fa-book fa-2x"></i>
   <small>Documentación</small>
 </a>
    </div>
</div>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
            <div class="container">
 
         <div class="row">      
        <div class="col-12">
              <div class="card bg-light card-danger">
                     <div class="card-header lead"></div>
                     <div class="card-body bg-white">
                        <div class="row ">
                            <div class="col-6">
                                <div class="row">
                                     <label class="control-label col-sm-3 text-right">Estatus</label>
                                     <div class="col-sm-8">
                                         <select id="SelectFilterEstatus" multiple onchange="bonosTransporte.OnSelectFilterEstatus()"  class="form-control form-control-sm selectpicker" >
                                        
                                             <option value="P"> EN PROCESO</option>
                                             <option value="F"> CERRADO</option>                                             
                                         </select>
                                     </div>
                                </div>
                                
                            </div>
                         </div> 

                         <div class="row mt-4">
                             <table id="tbBonosTransporteLotes" class="table table-bordered table-sm"> </table>
                         </div>
                     </div>
                     <div class="card-footer">
                         <div class="col-5 offset-6">                             
                         </div>
                     </div>
              </div>
        </div>
  </div>


      <%--modal--%>
          <div class="row">
            <div class="modal fade" id="modalAlta" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
                 <style>
                     .modal-ku {
                         width: 1250px !important;
                         margin: auto;
                     }
                 </style>
        
             <div class="modal-dialog modal-xl" role="document" idDialog="template">
                 <div class="modal-content">
                 <div class="modal-header">
                     <h5 class="modal-title" id="exampleModalLabel">Carga de bonos de transporte</h5>
                     <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                     <span aria-hidden="true">&times;</span>
                     </button>
                 </div>
                 <div class="modal-body">

                    <div class ="row">
                         <div class="col-3">
                            <div class="form-group row">
                                    <div class="col-sm-3 pñ-2">
                                        <label for="SelectSociedad" class="col-form-label">Sociedad</label>
                                    </div>                  
                                    <div class="col-sm-9">                                    
                                    <select id="SelectSociedad"  onchange="bonosTransporte.onChangeSociedad()"  class=" selectpicker form-control form-control-md" data-live-search="true" > </select>
                                    </div>
                                </div>
                        </div>

                          <div class="col-3">
                                    <div class="form-group row">
                                            <div class="col-sm-3 p-0">
                                                <label for="SelectDivision" class=" col-form-label">División</label>
                                            </div>
                
                                            <div class="col-sm-9">
                                            <select id="SelectDivision" onchange="bonosTransporte.validateForm()"  class=" selectpicker form-control form-control-md"  data-live-search="true" ></select>
                                            </div>
                                        </div>

                            </div>
                            <div class="col-3">
                                    <div class="form-group row">
                                            <div class="col-sm-3 p-0">
                                                <label for="SelectPeriodo" class=" col-form-label">Periodo</label>
                                            </div>
                
                                            <div class="col-sm-9">
                                            <select id="SelectPeriodo" onchange="bonosTransporte.validateForm()"  class="form-control form-control-md"  >
                                                <option> Division</option>
                                            </select>
                                            </div>
                                        </div>

                                </div>
                          
                       </div>

                    <div class="row">
                           <table id="tbBonosTransporteDetalle" class="table table-bordered table-sm w-100"> </table>
                    </div>
                    <div class="row mt-4" id="sectionCommentAut" >
                        <div class="col-12">
                            Comentario Autorización
                        </div>
                        <div class="col-12">
                            <div class="form-floating">
                            <textarea class="form-control" id="txtCommentAut"  placeholder="Ingresa un comentario" id="floatingTextarea2" style="height: 100px" maxlength="500"></textarea>                            
                            </div>

                        </div>
                    </div>
             
                 </div>
                 <div  class="modal-footer">
                     <div id="buttonSection" class="row">
                          <button id="btnRechazar" type="button" class="btn btn-danger mr-3" onclick=" bonosTransporte.UpsertBonosTransporte('R')"  >Rechazar</button>
                          <button id="btnAutorizar" type="button" class="btn btn-success mr-3"  onclick=" bonosTransporte.UpsertBonosTransporte('A')" >Autorizar</button>
                     </div>
                     
                 </div>
                 </div>
             </div>
             </div>
   </div>
</div>

   <script>
       var myWebServiceURL = "<%= Page.ResolveClientUrl("~/WebServices/WebServicesBonosTransporte.asmx")%>";
       var myWebServiceFiles = "<%= Page.ResolveClientUrl("~/WebServices/WebServicesBonosTransporte.asmx")%>";

       bonosTransporte = {};
       bonosTransporte.data = {
           bono: undefined,
           lotes: []
       }
       bonosTransporte.dt = {
           lotes: undefined,
           BonoDetail : undefined           
       }
       bonosTransporte.item = {
           SelectSociedad: document.getElementById("SelectSociedad"),
           SelectDivision: document.getElementById("SelectDivision"),
           SelectPeriodo: document.getElementById("SelectPeriodo"),
           btnRechazar: document.getElementById("btnRechazar"),
           btnAutorizar: document.getElementById("btnAutorizar"),
           txtCommentAut: document.getElementById("txtCommentAut")
           
       }
       bonosTransporte.user = {
           email: '<%= If(Session("User") IsNot Nothing, CType(Session("User"), ICMTools.User).Email, "")  %>',
           SocietyDivision: 0,
           sociedades: [],
           divisiones: [],
           periods: []
       }
       bonosTransporte.selectRow = undefined;
       bonosTransporte.functions = {

           distinct: (arr, attr1, attr2) => {
               const seen = new Set();
               const result = [];

               for (const item of arr) {
                   const key = `${item[attr1]}-${item[attr2]}`; 
                   if (!seen.has(key)) {
                       seen.add(key);
                       result.push(item);
                   }
               }
               return result;
           }
       }
       bonosTransporte.init = (data) => {
           config = {
               data: data,
               columns: [
                   { data: 'IDBono', title: 'Lote' },
                   { data: 'CreationEmployee', title: 'Solicitante' },
                   { data: 'DivsionName', title: 'División' },
                   { data: 'Periodo', title: 'Periodo' },
                   { data: 'CreationDate', title: 'Fecha Carga' },
                   { data: 'AuthorizedEmployee', title: 'Autorizador' },
                   { data: 'AuthorizedDate', title: 'Fecha Autorización' },
                   { data: 'StatusDescription', title: 'Estatus' },
                   { data: null, className: "text-center", title: 'Evento', defaultContent: '<i id="MessageIcon" class="fa fa-list-alt text-primary" ></i>', targets: -1 }
               ]
           };

           bonosTransporte.dt.lotes = icmTools.Datatable("tbBonosTransporteLotes", config);
           icmTools.hideLoading()
           bonosTransporte.dt.lotes.off('click', 'i').on('click', 'i', function (e) {
               let data = bonosTransporte.dt.lotes.row(e.target.closest('tr')).data();
               bonosTransporte.selectRow = data;
               bonosTransporte.getBonosTransporteDetail(data)
           });
       }
       bonosTransporte.MapDataTableDetail = (data) => {
           var config = {
               data: data.d,
               columns: [
                   { data: 'Payee', title: 'Empleado' },
                   { data: 'DateBono', title: 'Fecha' },
                   { data: 'CCNom', title: 'CCNom' },
                   { data: 'Amount', title: 'Monto' },
                   { data: 'Reason', title: 'Motivo ' },
                   {
                       className: "text-center",
                       data: 'StatusCode', title: 'Estatus ',
                       render: function (data, type, row, meta) {
                           switch (data) {
                               case "S":
                                   return '<i id="MessageIcon" class="fa fa-check text-warning" title ="' + row.MessageResponse + '"></i>';
                                   break;
                               case "P":
                                   return '<i id="MessageIcon" class="fa fa-check text-success" ></i>';
                                   break;
                               case "F":
                                   return '<i id="MessageIcon" class="fa fa-check-circle text-success" ></i>';
                                   break;
                               case "A":
                                   return '<i id="MessageIcon" class="fa fa-exclamation-triangle text-warning" title ="' + row.MessageResponse + '"></i>';
                                   break;
                               case "E":
                                   return '<i id="MessageIcon" class="fa fa-exclamation-circle text-danger" title="' + row.MessageResponse + '"></i>';
                                   break;
                               case "R":
                                   return '<i id="MessageIcon" class="fa fa-times text-danger" ></i>';
                                   break;
                           }

                           return '';
                       }
                   },
                   { data: 'MessageResponse', title: 'Descripción Estatus' }

               ]
           };
           bonosTransporte.dt.BonoDetail = icmTools.Datatable("tbBonosTransporteDetalle", config);


       }
       bonosTransporte.AltaModalEnabled = () => {
           bonosTransporte.data.bono = undefined;

           var SelectDivision = document.getElementById("SelectDivision")
           SelectDivision.removeAttribute("disabled", "")

           var SelectPeriodo = document.getElementById("SelectPeriodo")
           SelectPeriodo.removeAttribute("disabled", "")

       }
       bonosTransporte.AltaModalDisabled = () => {
           var SelectSociedad = document.getElementById("SelectSociedad")
           $('#' + SelectSociedad.id).prop('disabled', true);           

           var SelectDivision = document.getElementById("SelectDivision")
           $('#' + SelectDivision.id).prop('disabled', true);           

           var SelectPeriodo = document.getElementById("SelectPeriodo")
           $('#' + SelectPeriodo.id).prop('disabled', true);           

       }

       bonosTransporte.AltaModal = () => {
           $('#modalAlta').modal({ backdrop: 'static', keyboard: false }, 'show');
           bonosTransporte.AltaModalEnabled()

       }

       bonosTransporte.onChangeSociedad = () => {
           var SelectDivision = bonosTransporte.item.SelectDivision
           var SelectSociedad = bonosTransporte.item.SelectSociedad

           let user = bonosTransporte.user
           let filter = user.divisiones.filter(x => x.idSociedad == SelectSociedad.value)           

           SelectDivision.innerHTML = "";

           filter.forEach(division => {
               var option = document.createElement("option")
               option.value = division.idDivision
               option.text = division.division

               SelectDivision.append(option)
           });
           $('#' + SelectDivision.id).selectpicker('val', '');

           $('select').selectpicker("refresh");           
       }

       bonosTransporte.NuevoLote = () => {
           icmTools.showLoading();


           const func = () => {

               var SelectSociedad = bonosTransporte.item.SelectSociedad;
               var SelectPeriodo = bonosTransporte.item.SelectPeriodo;
               let index = 0;
               let user = bonosTransporte.user

               SelectSociedad.innerHTML = ""
               SelectDivision.innerHTML = ""
               SelectPeriodo.innerHTML = ""

               user.sociedades.forEach(sociedad => {
                   var option = document.createElement("option")
                   option.value = sociedad.idSociedad
                   option.text = sociedad.sociedad
                   if (index == 0) {
                       option.setAttribute("selected", "")
                   }
                   index++;
                   SelectSociedad.append(option)
               });

               user.periods.forEach(periodo => {
                   var option = document.createElement("option")
                   option.value = periodo.IDPeriod
                   option.text = periodo.PeriodName
                   if (index == 0) {
                       option.setAttribute("selected", "")
                   }
                   index++;
                   SelectPeriodo.append(option)
               });

               $('select').selectpicker("refresh");;

               SelectSociedad.value = SelectSociedad.firstChild.value

               bonosTransporte.onChangeSociedad();               
               icmTools.hideLoading();
           }           
           func()
       }

       bonosTransporte.getSocietyDivision = (id, func) => {
           let userValidate = false
           if (bonosTransporte.user.SocietyDivision > 0) {
               userValidate = bonosTransporte.user._activeEmail == bonosTransporte.user.email
           }
           if (bonosTransporte.user.SocietyDivision.length == 0 || !userValidate) {
               $.ajax({
                   type: "POST",
                   url: myWebServiceURL + "/getSocietyDivisionByID",                   
                   data: "{idDivision : '" + id + "'  }",
                   processData: true,
                   contentType: "application/json; charset=utf-8",
                   dataType: "json",
                   success: function (data) {
                       console.log("Respuesta recibida:");
                       if (data && data.d) {
                           console.log("Datos del DataTable:", data.d);
                           if (Array.isArray(data.d)) {
                               bonosTransporte.user.SocietyDivision = data.d.length
                               bonosTransporte.user.sociedades = [];
                               bonosTransporte.user.divisiones = [];
                               if (data.d.length > 0) {
                                   var SocietyDivision = structuredClone(data.d)
                                   var sociedades = SocietyDivision.map(x => ({ sociedad: x.sociedad, idSociedad: x.idSociedad }))
                                   bonosTransporte.user.sociedades = bonosTransporte.functions.distinct(sociedades, "sociedad", "idSociedad ")


                                   var divisiones = SocietyDivision.map(x => ({ idSociedad: x.idSociedad, idDivision: x.idDivision, division: x.division }))
                                   bonosTransporte.user.divisiones = bonosTransporte.functions.distinct(divisiones, "idDivision", "division ")

                               }
                               func();
                               icmTools.hideLoading();
                           }
                       } else {
                           console.log("Respuesta data.d vacía o no existe:", data);
                       }
                   },
                   error: function (XMLHttpRequest, textStatus, errorThrown) {
                       icmTools.hideLoading()
                       console.error("Error en la llamada AJAX a getDataInit:");
                       console.error("Estado del texto:", textStatus);
                       console.error("Error lanzado:", errorThrown);
                       console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); 
                   }
               });
           } else {
               func();
           }
       }

       bonosTransporte.getBonosTransporteDetail = (Bono) => {

           icmTools.showLoading("Obteniendo información")

           $.ajax({
               type: "POST",
               url: myWebServiceURL + "/getBonosTransporteDetail",
               data: "{idBono : '" + Bono.IDBono + "', onlyActive : '" + 1 + "'  }",
               processData: true,
               contentType: "application/json; charset=utf-8",
               dataType: "json",

               success: function (data) {
                   console.log("Respuesta recibida:");
                   if (data && data.d) {
                       bonosTransporte.MapDataTableDetail(data);

                       bonosTransporte.AltaModal()
                       bonosTransporte.AltaModalDisabled();
                       var SelectSociedad = bonosTransporte.item.SelectSociedad;
                       var SelectPeriodo = bonosTransporte.item.SelectPeriodo;
                       var SelectDivision = bonosTransporte.item.SelectDivision;
                       var txtCommentAut = bonosTransporte.item.txtCommentAut;


                       bonosTransporte.user.SocietyDivision = data.d.length
                       bonosTransporte.user.sociedades = [{ sociedad: Bono.Society, idSociedad: Bono.SocietyId }];
                       bonosTransporte.user.divisiones = [{ idSociedad: Bono.SocietyId, idDivision: Bono.DivisionID, division: Bono.DivsionName }];
                       bonosTransporte.NuevoLote();

                       bonosTransporte.data.bono = Bono;
                       bonosTransporte.item.btnAutorizar.removeAttribute("disabled");
                       bonosTransporte.item.btnRechazar.removeAttribute("disabled");
                       txtCommentAut.removeAttribute("disabled");
                       Bono = bonosTransporte.selectRow;
                       txtCommentAut.value = Bono.AuthorizedComment;
                       $('#' + SelectSociedad.id).val(Bono.SocietyId)
                       bonosTransporte.onChangeSociedad();
                       $('#' + SelectDivision.id).val(Bono.DivisionID)
                       $('#' + SelectPeriodo.id).val(Bono.PeriodoId)
                       $('select').selectpicker("refresh");

                       if (["R", "A", "F"].includes(Bono.StatusCode)) {
                           bonosTransporte.item.btnAutorizar.setAttribute("disabled", "");
                           bonosTransporte.item.btnRechazar.setAttribute("disabled", "");
                           txtCommentAut.setAttribute("disabled", "");
                       }

                       icmTools.hideLoading()
                       console.log("Respuesta recibida: tbBonosTransporteDetalle");
                   } else {
                       console.log("Respuesta data.d vacía o no existe:", data);
                   }
               },
               error: function (XMLHttpRequest, textStatus, errorThrown) {
                   icmTools.hideLoading()
                   console.error("Error en la llamada AJAX a getDataInit:");
                   console.error("Estado del texto:", textStatus);
                   console.error("Error lanzado:", errorThrown);
                   console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); 
               }
           });
       }

       bonosTransporte.UpsertBonosTransporte = (status) => {

           let bono = bonosTransporte.data.bono;           

           icmTools.showLoading("Validando información")
           let comment = bonosTransporte.item.txtCommentAut.value;           
           let idSociedad = bonosTransporte.item.SelectSociedad.value;
           let idDivision = bonosTransporte.item.SelectDivision.value;
           let idPeriod = bonosTransporte.item.SelectPeriodo.value;


           $.ajax({
               type: "POST",
               url: myWebServiceURL + "/UpsertBonosTransporte",               
               data: "{idBono : '" + bono.IDBono + "', statusBono : '" + status + "', comment : '" + comment + "', idSociedad : '" + idSociedad + "', idDivision : '" + idDivision + "', idPeriod : '" + idPeriod + "'}",
               processData: true,
               contentType: "application/json; charset=utf-8",
               dataType: "json",

               success: function (data) {
                   console.log("Respuesta recibida:");
                   if (data && data.d) {
                       bonosTransporte.getDataInit();
                       console.log("Respuesta recibida: tbBonosTransporteDetalle");
                   } else {
                       console.log("Respuesta data.d vacía o no existe:", data);
                   }
               },
               error: function (XMLHttpRequest, textStatus, errorThrown) {
                   icmTools.hideLoading()
                   console.error("Error en la llamada AJAX a getDataInit:");
                   console.error("Estado del texto:", textStatus);
                   console.error("Error lanzado:", errorThrown);
                   console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); 
               }
           });
       }

       bonosTransporte.OnSelectFilterEstatus = () => {
           var data = structuredClone(bonosTransporte.data.lotes);
           var select = document.getElementById("SelectFilterEstatus")
           var filter = structuredClone(data);
           var selects = $('#' + select.id).val();
           if (select.value !== "T" && select.value !== "") {
               filter = data.filter(x => selects.includes(x.StatusCode));
           }
           bonosTransporte.init(filter);
       }
       bonosTransporte.getDataInit = () => {

           $.ajax({
               type: "POST",
               url: myWebServiceURL + "/getDataInit",
               data: "{_type : 'AUTHORIZED'  }",
               processData: true,
               contentType: "application/json; charset=utf-8",
               dataType: "json",

               success: function (data) {
                   console.log("Respuesta recibida:");
                   if (data && data.d) {
                       console.log("Datos del DataTable:", data.d);
                       if (Array.isArray(data.d)) {
                           $('#modalAlta').modal('hide');
                           bonosTransporte.data.lotes = data.d;
                           bonosTransporte.init(data.d);
                       }
                   } else {
                       console.log("Respuesta data.d vacía o no existe:", data);
                   }
               },
               error: function (XMLHttpRequest, textStatus, errorThrown) {
                   icmTools.hideLoading()
                   console.error("Error en la llamada AJAX a getDataInit:");
                   console.error("Estado del texto:", textStatus);
                   console.error("Error lanzado:", errorThrown);
                   console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText);                    
               }
           });
       }
       bonosTransporte.getPeriod = (func) => {
           let userValidate = false
           if (bonosTransporte.user.SocietyDivision > 0) {
               userValidate = bonosTransporte.user._activeEmail == bonosTransporte.user.email
           }
           if (bonosTransporte.user.SocietyDivision.length == 0 || !userValidate) {
               $.ajax({
                   type: "POST",
                   url: myWebServiceURL + "/getPeriod",
                   data: "{}",
                   processData: true,
                   contentType: "application/json; charset=utf-8",
                   dataType: "json",
                   success: function (data) {
                       console.log("Respuesta recibida:");
                       if (data && data.d) {
                           console.log("Datos del DataTable:", data.d);
                           if (Array.isArray(data.d)) {
                               bonosTransporte.user.periods = data.d
                               if (func) {
                                   func();
                               }
                           }
                       } else {
                           console.log("Respuesta data.d vacía o no existe:", data);
                       }
                   },
                   error: function (XMLHttpRequest, textStatus, errorThrown) {
                       icmTools.hideLoading()
                       console.error("Error en la llamada AJAX a getDataInit:");
                       console.error("Estado del texto:", textStatus);
                       console.error("Error lanzado:", errorThrown);
                       console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); 
                   }
               });
           } else {
               func();
           }
       }
       window.onload = () => {
           icmTools.showLoading()

           const func = () => {               
               bonosTransporte.getDataInit();
           }
           bonosTransporte.getPeriod(func)
       }
   </script>
</asp:Content>
