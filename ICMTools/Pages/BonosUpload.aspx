<%@ Page Title="Carga de Bonos" Language="vb" AutoEventWireup="false" MasterPageFile="~/Master/MasterPage.Master" CodeBehind="BonosUpload.aspx.vb" Inherits="ICMTools.BonosUpload" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<%@ MasterType VirtualPath="~/Master/MasterPage.Master"%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
      <meta name="description" content="Form para Importación de Excepciones con procesos de validación"/>
      <meta name="author" content="Pedro Antonio Cardona"/>
      <title>Carga de Bonos</title>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="TopbarContent" runat="server">
   <div class="container-fluid">
<div class="d-flex gap-1">
      <a href="../Pages/BonosUpload.aspx" class="btn active btn-sm btn-bar d-flex flex-column align-items-center text-dark">
        <i class="fas fa-upload fa-2x"></i>
        <small>Carga</small>
      </a>   
      
      <a href="../Pages/BonosAuthorization.aspx"  class="btn btn-sm btn-bar d-flex flex-column align-items-center text-dark">
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
                                          <select id="SelectFilterEstatus" onchange="bonosTransporte.OnSelectFilterEstatus()"  class="form-control form-control-sm selectpicker" multiple >                                            
                                              <option value="P"> EN PROCESO</option>
                                              <option value="F"> CERRADO</option>                                              
                                          </select>
                                      </div>
                                 </div>                                 
                             </div>
                             <div class="col-5">                                 
                                 <button id="btAltaBonos" class="btn btn-sm btn-success float-right" data-toggle="tooltip" data-placement="top" title="" onclick="bonosTransporte.NuevoLote()">Cargar Bonos de transporte</button>
                             </div>
                          </div> 
                          <div class="row" style="margin-top: 35px">
                             <table id="tbBonosTransporteLotes" class="table table-bordered table-sm"> </table>
                          </div>                         
                      </div>
                      <div class="card-footer">
                          <div class="col-5 offset-6">                             
                              <a href="../TemplateFiles/ICMToolsPlantilla_BonosTransporte.xlsx" class="btn btn-sm btn-primary float-right"><i class="fas fa-download fa-fw"></i>Descargar Plantilla</a>
                          </div>
                      </div>
               </div>
         </div>
   </div>
 <form runat="server">       
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
                            <div class="col-3">
                                  
                                   <div class="row" id="fileUploadButton">
                                     <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>                                         
                                         <div class="upload-drop-zone" id="drop-zone">
                                             <ajaxToolkit:AsyncFileUpload ID="AsyncFileUpload1" runat="server"  ThrobberID="myThrobber" OnUploadedComplete="AsyncFileUpload1_UploadedComplete" OnClientUploadComplete="uploadComplete" OnClientUploadError="uploadError" OnClientUploadStarted="beforeUploadStarts" Width="100%" ErrorBackColor="#FFCCFF" CompleteBackColor="#CCFFCC" ForeColor="Black" />                                             
                                         </div>
                                         <asp:Label runat="server" ID="myThrobber" Style="display: none;"><i class="fas fa-sync-alt fa-spin fa-fw"></i>Cargando archivo...</asp:Label>                                         
                                 </div>                                 
                            </div>
                        </div>

                     <div class="row col-12">
                         <div id="accordion" class="w-100">

                          <div class="card">
                            <div class="card-header">
                              <a class="card-link" data-toggle="collapse" href="#collapseOne" id="tabTitleSinAut" >
                                 Registros sin autorización <span class="badge badge-pill badge-info" id="badgeSinAut">4</span>
                              </a>
                            </div>
                            <div id="collapseOne" class="collapse show" >
                              <div class="card-body">
                                     <table id="tbBonosTransporteDetalle" class="table table-bordered table-sm w-100"> </table>
                              </div>
                            </div>
                          </div>

                          <div class="card">
                            <div class="card-header">
                              <a class="collapsed card-link" data-toggle="collapse" href="#collapseTwo" id="tabTitleAut">
                                Registros con autorización <span class="badge badge-pill badge-info" id="badgeAut">4</span>
                              </a>
                            </div>
                            <div id="collapseTwo" class="collapse" >
                              <div class="card-body">
                                <table id="tbBonosTransporteDetalleAut" class="table table-bordered table-sm w-100"> </table>
                              </div>
                            </div>
                          </div>

                          <div class="card">
                            <div class="card-header">
                              <a class="collapsed card-link" data-toggle="collapse" href="#collapseThree" id="tabTitleEx">
                                Registros excluidos <span class="badge badge-pill badge-info" id="badgeEx">4</span>
                              </a>
                            </div>
                            <div id="collapseThree" class="collapse" data-parent="#accordion">
                              <div class="card-body">
                              <table id="tbBonosTransporteDetalleEx" class="table table-bordered table-sm w-100"> </table>
                              </div>
                            </div>
                          </div>
                        </div>
                     </div>
                     <div class="row mt-4">
                                <div class="col-12">
                                    Comentario Alta
                                </div>
                                <div class="col-12">
                                   <div class="form-floating">
                                    <textarea class="form-control" id="txtComment" placeholder="Ingresa un comentario" id="floatingTextarea2" style="height: 100px" maxlength="500"></textarea>                                    
                                    </div>
                                </div>
                     </div>

                      <div class="row mt-4" id="sectionCommentAut" >
                            <div class="col-12">
                                Comentario Autorización 
                            </div>
                            <div class="col-12">
                                <div class="form-floating">
                                <textarea class="form-control" id="txtCommentAut" disabled placeholder="Ingresa un comentario" id="floatingTextarea2" style="height: 100px" maxlength="500"></textarea>                                
                                </div>
                            </div>
                        </div>
              
                  </div>
                  <div  class="modal-footer">
                      <div id="buttonSection" class="row">
                         <div class="col">
                              <button type="button" class="btn btn-secondary" data-dismiss="modal">Cerrar</button>
                         </div>
                          <div class="col">
                              <button id="btnCarga" type="button" class="btn btn-success" disabled  onclick="bonosTransporte.UploadValidations()">Generar carga</button>
                          </div>                           
                      </div>                      
                  </div>
                  </div>
              </div>
              </div>
            </div>
     
   </form>

   
        



     



 </div>

   

    <script>

        function uploadError(sender, args) {
            icmTools.hideLoading()

            var text = "<span class='text-danger'>" + args.get_errorMessage() + "</span>";
            //addToClientTable(args.get_fileName(), text);
        }

        function uploadComplete(sender, args) {
            var contentType = args.get_contentType();
            //var text = "<span class='text-success'>Listo para validar!</span>";
            //addToClientTable(args.get_fileName() + " " +args.get_length() + " bytes", text);
            File = args.get_fileName();
            $('#btnStartImport').prop('disabled', false);

            bonosTransporte.SaveDataByDocument(File)


            //setFormStatus('newfile');
        }

        function beforeUploadStarts(sender, args) {
            icmTools.showLoading("Cargando documento")



            $("#statusUploadTable").html("");
            var content = document.getElementById("ContentPlaceHolder1_AsyncFileUpload1_ctl01")
            var input = content.getElementsByTagName("input")[0]


            var fileSize = input.files[0].size;
            var maxFileSize = "<%=ConfigurationManager.AppSettings("maxFileSize")%>"
            if (fileSize > maxFileSize) {
                throw {
                    name: "Invalid File Size",
                    level: "Error",
                    message: "File muy pesado (máximo " + (maxFileSize / 1048576).toFixed(2) + " Mb)",
                    htmlMessage: "Invalid File Size (maximum " + (maxFileSize / 1048576).toFixed(2) + " Mb)"
                }
                return false;
            }

            var filename = args.get_fileName();
            var ext = filename.substring(filename.lastIndexOf(".") + 1);
            if (ext != "xlsx") {
                throw {
                    name: "Invalid File Type",
                    level: "Error",
                    message: "Tipo de archivo invalido (Solo .xlsx)",
                    htmlMessage: "Invalid File Type (Only .xlsx)"
                }
                return false;
            }
            return true;
        }

        var myWebServiceURL = "<%= Page.ResolveClientUrl("~/WebServices/WebServicesBonosTransporte.asmx")%>";
       var myWebServiceFiles = "<%= Page.ResolveClientUrl("~/WebServices/WebServicesBonosTransporte.asmx")%>";

        bonosTransporte = {};
        bonosTransporte.item = {
            SelectSociedad :  document.getElementById("SelectSociedad"),
            SelectDivision :  document.getElementById("SelectDivision"),
            SelectPeriodo: document.getElementById("SelectPeriodo"),
            txtComment: document.getElementById("txtComment"),
            sectionCommentAut: document.getElementById("sectionCommentAut"),
            txtCommentAut: document.getElementById("txtCommentAut"),
            badgeSinAut: document.getElementById("badgeSinAut"),
            badgeAut: document.getElementById("badgeAut"),
            badgeEx: document.getElementById("badgeEx"),
            tabTitleSinAut: document.getElementById("tabTitleSinAut"),
            tabTitleAut: document.getElementById("tabTitleAut"),
            tabTitleEx: document.getElementById("tabTitleEx")
        }
        bonosTransporte.user = {
            email: '<%= If(Session("User") IsNot Nothing, CType(Session("User"), ICMTools.User).Email, "")  %>',
            SocietyDivision: 0,
            sociedades: [],
            divisiones: [],
            periods: [],
            Authorizer: null
        }
        bonosTransporte.data = {
            bono: undefined,
            lotes: []
        }
        bonosTransporte.dt = {
            lotes: undefined,
            BonoDetail: undefined,
            BonoDetailAut: undefined,
            BonoDetailEx: undefined

        }
        bonosTransporte.functions = {

            distinct: (arr, attr1, attr2) => {
                const seen = new Set();
                const result = [];

                for (const item of arr) {
                    const key = `${item[attr1]}-${item[attr2]}`; // Crea una clave única
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
                order: [[7, 'desc'], [0, 'asc']],
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

            bonosTransporte.dt.lotes.off('click', 'i').on('click', 'i', function (e) {
                let data = bonosTransporte.dt.lotes.row(e.target.closest('tr')).data();
                bonosTransporte.data.bono = undefined;
                bonosTransporte.NuevoLote();
                bonosTransporte.getBonosTransporteDetail(data)

            });

            icmTools.hideLoading()

        }
        bonosTransporte.MapDataTableDetail = (data, readOnly) => {

            const createob = (_data) => {
                return {
                    data: _data,
                    columns: [
                        { data: 'Payee', title: 'Empleado' },
                        { data: 'DateBono', title: 'Fecha' },
                        { data: 'CCNom', title: 'CCNom' },
                        {
                            data: 'Amount', title: 'Monto',
                            render: function (data, type, row, meta) {
                                if (Number(data).toString() !== 'NaN') {
                                    let amount = Number(data)

                                    return new Intl.NumberFormat('en-US', {
                                        style: 'currency',
                                        currency: 'USD'
                                    }).format(amount);

                                }

                                return data
                            }

                        },
                        { data: 'Reason', title: 'Motivo ' },
                        {
                            className: "text-center",
                            data: 'StatusCode', title: 'Estatus ',
                            render: function (data, type, row, meta) {
                                switch (data) {
                                    case "R":
                                        return '<i id="MessageIcon" class="fa fa-times text-danger" ></i>';
                                        break;
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
                                }

                                return '';
                            }

                        },
                        { data: 'MessageResponse', title: 'Descripción Estatus' }

                    ]
                };


            }
            bonosTransporte.item.badgeAut.setAttribute("style", "display:none")
            bonosTransporte.item.badgeSinAut.setAttribute("style", "display:none")
            bonosTransporte.item.badgeEx.setAttribute("style", "display:none")

            bonosTransporte.item.tabTitleSinAut.firstChild.textContent = "Registros sin autorización "
            bonosTransporte.item.tabTitleAut.firstChild.textContent = "Registros con autorización "
            bonosTransporte.item.tabTitleEx.firstChild.textContent = "Registros excluidos "


            if (!data.d) data.d = []

            let dataDetail = [];
            let dataDetailAut = [];
            let dataDetailEx = [];
            let bono = bonosTransporte.data.bono

            if (!readOnly) {
                dataDetail = createob(data.d.filter(x => x.StatusCode == "P"));
                dataDetailAut = createob(data.d.filter(x => ["A", "S"].includes(x.StatusCode)));
                dataDetailEx = createob(data.d.filter(x => x.StatusCode == "E"));
            } else {
                dataDetail = createob(data.d.filter(x => x.StatusCode == "F"));
                dataDetailAut = createob(data.d.filter(x => ["A", "S"].includes(x.StatusCode)));
                dataDetailEx = createob(data.d.filter(x => x.StatusCode == "E"));

                bonosTransporte.item.tabTitleSinAut.firstChild.textContent = "Registros Cargados "
                bonosTransporte.item.tabTitleAut.firstChild.textContent = "Registros pendientes de autorización"
                bonosTransporte.item.tabTitleEx.firstChild.textContent = "Registros excluidos "

                if (bono && bono.StatusCode == "F") {
                    dataDetailAut = createob(data.d.filter(x => ["R"].includes(x.StatusCode)));
                    bonosTransporte.item.tabTitleAut.firstChild.textContent = "Registros Rechazados"
                }
            }

            if (data.d.length > 0) {
                bonosTransporte.item.badgeAut.removeAttribute("style")
                bonosTransporte.item.badgeAut.textContent = dataDetailAut.data.length
                bonosTransporte.item.badgeSinAut.removeAttribute("style")
                bonosTransporte.item.badgeSinAut.textContent = dataDetail.data.length
                bonosTransporte.item.badgeEx.removeAttribute("style")
                bonosTransporte.item.badgeEx.textContent = dataDetailEx.data.length
            }




            //dataDetail.data = data.d.filter(x => x.StatusCode == "P")
            //dataDetailAut.data = data.d.filter(x => ["A", "S"].includes(x.StatusCode))
            //dataDetailEx.data = data.d.filter(x => x.StatusCode == "E")


            bonosTransporte.dt.BonoDetail = icmTools.Datatable("tbBonosTransporteDetalle", dataDetail);
            bonosTransporte.dt.BonoDetailAut = icmTools.Datatable("tbBonosTransporteDetalleAut", dataDetailAut);
            bonosTransporte.dt.BonoDetailEx = icmTools.Datatable("tbBonosTransporteDetalleEx", dataDetailEx);

        }
        bonosTransporte.AltaModalEnabled = () => {

            var SelectSociedad = document.getElementById("SelectSociedad")
            $('#' + SelectSociedad.id).prop('disabled', false);

            var SelectDivision = document.getElementById("SelectDivision")
            $('#' + SelectDivision.id).prop('disabled', false);


            var SelectPeriodo = document.getElementById("SelectPeriodo")
            $('#' + SelectPeriodo.id).prop('disabled', false);


            var btnCarga = document.getElementById("btnCarga")
            btnCarga.parentElement.setAttribute("class", "col ")

            bonosTransporte.item.txtComment.removeAttribute("disabled")


            var classItem = bonosTransporte.item.sectionCommentAut.getAttribute("class")
            classItem = classItem + " d-none"
            bonosTransporte.item.sectionCommentAut.setAttribute("class", classItem.trim())


            $('select').selectpicker("refresh");
        }
        bonosTransporte.AltaModalDisabled = () => {
            var fileUploadButton = document.getElementById("fileUploadButton")
            var inputs = fileUploadButton.getElementsByTagName("input")
            for (var i = 0; i < inputs.length; i++) {
                inputs[i].setAttribute("disabled", "")
            }

            var SelectSociedad = document.getElementById("SelectSociedad")
            $('#' + SelectSociedad.id).prop('disabled', true);
            //SelectDivision.setAttribute("disabled", "")

            var SelectDivision = document.getElementById("SelectDivision")
            $('#' + SelectDivision.id).prop('disabled', true);
            //SelectDivision.setAttribute("disabled", "")

            var SelectPeriodo = document.getElementById("SelectPeriodo")
            $('#' + SelectPeriodo.id).prop('disabled', true);
            //SelectPeriodo.setAttribute("disabled", "")

            var btnCarga = document.getElementById("btnCarga")
            btnCarga.parentElement.setAttribute("class", "col d-none")

            bonosTransporte.item.txtComment.setAttribute("disabled", "")

        }
        bonosTransporte.NuevoLote = () => {
            icmTools.showLoading();
            var fileUploadButton = document.getElementById("fileUploadButton")
            var inputs = fileUploadButton.getElementsByTagName("input")
            for (var i = 0; i < inputs.length; i++) {
                inputs[i].setAttribute("disabled", "")
            }

            const func = () => {
                document.getElementById("btnCarga").setAttribute("disabled", "")
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

                if (SelectSociedad.firstChild) SelectSociedad.value = SelectSociedad.firstChild.value

                bonosTransporte.onChangeSociedad();
                bonosTransporte.MapDataTableDetail([])


                $('#collapseOne').collapse('show')
                $('#collapseTwo').collapse('hide')
                $('#collapseThree').collapse('hide')


                bonosTransporte.AltaModal();
                icmTools.hideLoading();
            }
            bonosTransporte.getSocietyDivision(func)
        }
        bonosTransporte.AltaModalResultado = () => {
            $('#modalAlertaResultado').modal({ backdrop: 'static', keyboard: false }, 'show');
            $('#modalAlertaResultado').off('hidden.bs.modal').on('hidden.bs.modal', function () {
                $('#modalAlta').show()
            })

            $('#modalAlertaResultado').off('shown.bs.modal').on('shown.bs.modal', function (e) {
                $('#modalAlta').hide()
            })

        }
        bonosTransporte.AltaModal = () => {
            $('#modalAlta').modal({ backdrop: 'static', keyboard: false }, 'show');
            bonosTransporte.AltaModalEnabled()
        }
        bonosTransporte.getBonosTransporteDetail = (Bono) => {
            icmTools.showLoading("Obteniendo información")
            /*globaBono = Bono;*/
            bonosTransporte.data.bono = Bono;
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/getBonosTransporteDetail",
                data: "{idBono : '" + Bono.IDBono + "', onlyActive : '" + 0 + "'  }",
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",

                success: function (data) {
                    console.log("Respuesta recibida:");
                    if (data && data.d) {
                        bonosTransporte.MapDataTableDetail(data, true);

                        bonosTransporte.AltaModal()
                        bonosTransporte.AltaModalDisabled();

                        Bono = bonosTransporte.data.bono
                        var SelectSociedad = bonosTransporte.item.SelectSociedad;
                        var SelectPeriodo = bonosTransporte.item.SelectPeriodo;
                        var SelectDivision = bonosTransporte.item.SelectDivision;
                        var txtCommentAut = bonosTransporte.item.txtCommentAut;

                        txtCommentAut.value = Bono.AuthorizedComment;
                        var classItem = bonosTransporte.item.sectionCommentAut.getAttribute("class")
                        classItem = classItem.replaceAll("d-none", "")
                        bonosTransporte.item.sectionCommentAut.setAttribute("class", classItem.trim())
                        $('#' + SelectSociedad.id).val(Bono.SocietyId)
                        bonosTransporte.onChangeSociedad();
                        $('#' + SelectDivision.id).val(Bono.DivisionID)
                        $('#' + SelectPeriodo.id).val(Bono.PeriodoId)
                        $('select').selectpicker("refresh");

                        icmTools.hideLoading()
                        console.log("Respuesta recibida: tbBonosTransporteDetalle");
                    } else {
                        console.log("Respuesta data.d vacía o no existe:", data);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    console.log("Error en la llamada AJAX a getDataInit:");
                    console.log("Estado del texto:", textStatus);
                    console.log("Error lanzado:", errorThrown);
                    console.log("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error

                }
            });
        }
        bonosTransporte.onclick_btnCarga = () => {
            let success = data.d.filter(x => x.StatusCode == "S" || x.StatusCode == "A")
            let error = data.d.filter(x => x.StatusCode == "E")
            let passed = data.d.filter(x => x.StatusCode == "P")

            if (error.length == 0 && passed == 0) {

            }

        }
        bonosTransporte.UploadValidations = () => {
            let bono = bonosTransporte.data.bono;
            let idSociedad = bonosTransporte.item.SelectSociedad.value;
            let idDivision = bonosTransporte.item.SelectDivision.value;
            let idPeriod = bonosTransporte.item.SelectPeriodo.value;

            icmTools.showLoading("Validando información")

            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/UploadValidations",
                data: "{idBono : '" + bono.IDBono + "',idDivision : '" + idDivision + "',idSociety : '" + idSociedad + "'}",
                //data: "{idBono : '" + bono.IDBono + "', idDivision : '" + status + "', comment : '" + comment + "', idSociedad : '" + idSociedad + "', idDivision : '" + idDivision + "', idPeriod : '" + idPeriod + "'}",
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    console.log("Respuesta recibida:");
                    if (data && data.d && data.d.length > 0 && data.d[0].Value == "") {
                        /* bonosTransporte.getDataInit();*/
                        bonosTransporte.user.Authorizer = data.d[1].Value;
                        bonosTransporte.UpsertBonosTransporte();
                        console.log("Respuesta recibida: tbBonosTransporteDetalle");
                    } else {
                        icmTools.hideLoading();
                        icmTools.AlertError(data.d[0].Value)
                        console.log("Respuesta data.d vacía o no existe:", data);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    let msg = XMLHttpRequest.responseJSON.Message;
                    icmTools.AlertError('<strong>¡Error!</strong> ' + msg);
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error

                }
            });
        }

        bonosTransporte.UpsertBonosTransporte = () => {
            let bono = bonosTransporte.data.bono;
            let status = 'P'
            icmTools.showLoading("Registrando información")
            let comment = bonosTransporte.item.txtComment.value;
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
                        icmTools.hideLoading()
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    let msg = XMLHttpRequest.responseJSON.Message;
                    icmTools.AlertError('<strong>¡Error!</strong> ' + msg);

                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    // $("#errorDisplayDiv").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        bonosTransporte.validateFile = (Bono) => {
            icmTools.showLoading("Validando información")
            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/validateFile",
                data: "{idBono : '" + Bono.IDBono + "' }",
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",

                success: function (data) {
                    console.log("Respuesta recibida:");
                    console.log(data.d)
                    if (data && data.d) {
                        bonosTransporte.MapDataTableDetail(data);
                        //Poniendo P, de procesados.
                        let success = data.d.filter(x => x.StatusCode == "S" || x.StatusCode == "A" || x.StatusCode == "P")

                        if (success.length > 0) {
                            document.getElementById("btnCarga").removeAttribute("disabled")
                        }

                        let error = data.d.filter(x => x.StatusCode == "E")

                        if (error.length > 0) {
                            document.getElementById("btnCarga").setAttribute("disabled", "true");
                        }


                        icmTools.hideLoading()
                        console.log("Respuesta recibida: tbBonosTransporteDetalle");
                    } else {
                        console.log("Respuesta data.d vacía o no existe:", data);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    let msg = XMLHttpRequest.responseJSON.Message;
                    icmTools.AlertError('<strong>¡Error!</strong> ' + msg);
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    // $("#errorDisplayDiv").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        bonosTransporte.SaveDataByDocument = (File) => {
            icmTools.showLoading("Guardando información")
            let extension = File.split(".")[1]
            let idSociedad = bonosTransporte.item.SelectSociedad.value;
            let idDivision = bonosTransporte.item.SelectDivision.value;
            let idPeriod = bonosTransporte.item.SelectPeriodo.value;
            let comment = bonosTransporte.item.txtComment.value

            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/saveFile",
                data: "{extension : '" + extension + "', idSociedad : '" + idSociedad + "' , idDivision : '" + idDivision + "'  , idPeriod : '" + idPeriod + "'  , comment : '" + comment + "'  }",
                processData: true,
                contentType: "application/json; charset=utf-8",
                dataType: "json",

                success: function (data) {
                    console.log("Respuesta recibida:");
                    if (data && data.d) {
                        bonosTransporte.data.bono = data.d;
                        bonosTransporte.validateFile(data.d)
                    } else {
                        console.log("Respuesta data.d vacía o no existe:", data);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    let msg = XMLHttpRequest.responseJSON.Message;
                    icmTools.AlertError('<strong>¡Error!</strong> ' + msg);

                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    // $("#errorDisplayDiv").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }

        bonosTransporte.getSocietyDivision = (func) => {
            let userValidate = false
            if (bonosTransporte.user.SocietyDivision > 0) {
                userValidate = bonosTransporte.user._activeEmail == bonosTransporte.user.email
            }
            if (bonosTransporte.user.SocietyDivision.length == 0 || !userValidate) {
                $.ajax({
                    type: "POST",
                    url: myWebServiceURL + "/getSocietyDivision",
                    data: "{}",
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
                            }
                        } else {
                            console.log("Respuesta data.d vacía o no existe:", data);
                        }
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        icmTools.hideLoading()
                        let msg = XMLHttpRequest.responseJSON.Message;
                        icmTools.AlertError('<strong>¡Error!</strong> ' + msg);
                        console.error("Error en la llamada AJAX a getDataInit:");
                        console.error("Estado del texto:", textStatus);
                        console.error("Error lanzado:", errorThrown);
                        console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    }
                });
            } else {
                func();
            }
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
                        let msg = XMLHttpRequest.responseJSON.Message;
                        icmTools.AlertError('<strong>¡Error!</strong> ' + msg);

                        console.error("Error en la llamada AJAX a getDataInit:");
                        console.error("Estado del texto:", textStatus);
                        console.error("Error lanzado:", errorThrown);
                        console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    }
                });
            } else {
                func();
            }
        }

        bonosTransporte.getDataInit = () => {

            $.ajax({
                type: "POST",
                url: myWebServiceURL + "/getDataInit",
                data: "{_type : 'CREATION'  }",
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
                        icmTools.hideLoading()
                    }

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    icmTools.hideLoading()
                    let msg = XMLHttpRequest.responseJSON.Message;
                    icmTools.AlertError('<strong>¡Error!</strong> ' + msg);
                    console.error("Error en la llamada AJAX a getDataInit:");
                    console.error("Estado del texto:", textStatus);
                    console.error("Error lanzado:", errorThrown);
                    console.error("Respuesta del servidor (cruda):", XMLHttpRequest.responseText); // Muestra el XML que está causando el error
                    // Aquí puedes mostrar un mensaje de error al usuario
                    // $("#errorDisplayDiv").html("Error: " + XMLHttpRequest.responseText);
                }
            });
        }
        bonosTransporte.validateForm = () => {
            var items = bonosTransporte.item
            let idDivision = items.SelectDivision.value;
            let idSociedad = items.SelectSociedad.value;
            let idPeriodo = items.SelectPeriodo.value;
            var fileUploadButton = document.getElementById("fileUploadButton")
            var inputs = fileUploadButton.getElementsByTagName("input")

            if (idDivision == "" || idSociedad == "" || idPeriodo == "") {
                for (var i = 0; i < inputs.length; i++) {
                    inputs[i].setAttribute("disabled", "")
                }
                return false
            }

            for (var i = 0; i < inputs.length; i++) {
                inputs[i].removeAttribute("disabled", "")
                inputs[i].value = "";

            }
            inputs[1].setAttribute("style", "width: 100%");

            bonosTransporte.MapDataTableDetail([])
            $('#collapseOne').collapse('show')
            $('#collapseTwo').collapse('hide')
            $('#collapseThree').collapse('hide')

            return true
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
        bonosTransporte.onChangeSociedad = () => {
            var SelectDivision = bonosTransporte.item.SelectDivision
            var SelectSociedad = bonosTransporte.item.SelectSociedad


            let user = bonosTransporte.user
            let filter = user.divisiones.filter(x => x.idSociedad == SelectSociedad.value)

            //SelectDivision.innerHTML = "<option disabled selected value> </option>";

            SelectDivision.innerHTML = "";

            filter.forEach(division => {
                var option = document.createElement("option")
                option.value = division.idDivision
                option.text = division.division

                SelectDivision.append(option)
            });
            $('#' + SelectDivision.id).selectpicker('val', '');

            $('select').selectpicker("refresh");
            bonosTransporte.validateForm()

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


<asp:Content ID="ModalAlta_PlaceHolder" ContentPlaceHolderID="Modal" runat="server">
        
          
              
         <div id ="ModalBaja" modal-title ="Titulo en ves de encabezado">
                <div modal-id ="Body">
                    Este es el cuerpo
                </div>
                <div modal-id ="Foot">
                    Este es el pie
                </div>
        </div>
</asp:Content>
