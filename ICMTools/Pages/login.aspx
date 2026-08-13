<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="login.aspx.vb" Inherits="ICMTools.login" Async="true"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta charset="utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no"/>
    <meta name="description" content="Form para Login a ICMTools"/>
    <meta name="author" content="Rousbelt Damian Garza Villarreal"/>
    <link rel="icon" type="image/png" href="../images/logo_icmtools_small.png" />
    <title>ICM Tools - Ingreso</title>

    <!-- Custom CSS -->
    <link href="../dist/css/custom.css" rel="stylesheet" />
    <!-- jQuery -->
    <script src="../vendor/jquery/jquery.min.js"></script>
    <!-- Bootstrap Core CSS -->
    <link href="../vendor/bootstrap-4.1.0/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <!-- Custom Fonts -->
    <link href="../vendor/font-awsome-free-5.0.10/css/fontawesome-all.min.css" rel="stylesheet" type="text/css" />    
</head>
<body class="bg-light">
    <div class="container">
        <div class="row">
            <div class="col-md-4 offset-md-4">
                <div class="card bg-defualt mt-5">
                    <div class="card-header">
                        <h1 class="h3 font-weight-normal">ICM Tools</h1>
                    </div>
                    <div class="card-body">
                        <div class="text-center">
                            <img src="../images/logo_icmtools.png" style="width: 100px;" />
                        </div>

                        <div id="Message" runat="server" class="alert alert-dismissible alert-success" role="alert" style="margin-top: 20px;">
                            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>

                            <h5 class="card-title">
                                <i id="MessageIcon" runat="server" class="fa fa-bomb"></i>
                                <span id="MessageTitle" runat="server" class="alert-heading">Mensaje titulo</span>
                            </h5>

                            <p id="MessagePrimary" runat="server">Mensaje primario.</p>
                            <hr />
                            <p id="MessageSecondary" runat="server" class="mb-0">Mensaje secundario.</p>
                        </div>
                        <a href="<%= ConfigurationManager.AppSettings("ICMUrl") %>" class="btn btn-outline-info btn-block"><i class="fas fa-undo-alt fa-fw"></i>Ir a Portal ICM Web</a>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Bootstrap Core JavaScript -->
    <script src="../vendor/bootstrap-4.1.0/dist/js/bootstrap.min.js"></script>
</body>
</html>