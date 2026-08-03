<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="logout.aspx.vb" Inherits="ICMTools.logout" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge"/>
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no"/>
    <meta name="description" content=""/>
    <meta name="author" content=""/>
    <title>ICM Tools - Logout</title>
    
    <!-- jQuery -->
    <script src="../vendor/jquery/jquery.min.js"></script>
    <!-- Bootstrap Core CSS--> 
    <link href="../vendor/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <!-- Custom CSS -->
    <link href="../dist/css/sb-admin-2.css" rel="stylesheet" />
    <!-- Custom Fonts -->
    <link href="../vendor/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css" />

        <script type="text/javascript" >

            function redireccionarPagina() {
                window.location = "<%= System.Configuration.ConfigurationManager.AppSettings("ICMUrl") %>";
            }            
            $(document).ready(function () {
                setTimeout("redireccionarPagina()", 5000);
            });
        </script>

</head>



<body class="login">

    <div class="container">
        <div class="row">
            <div class="col-md-4">
            </div>
            <div class="col-md-4">
                <div class="login-panel panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">ICM Tools</h3>
                    </div>
                    <div class="panel-body">
                        <div class="text-center">
                            <img src="../images/logo_xpertal.png" style="width: 200px;" />
                        </div>
                        <div id="Message" runat="server" class="alert alert-success" role="alert" style="margin-top: 20px;">
                            <p>Session Cerrada</p>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
            </div>
        </div>
    </div>

    <!-- jQuery -->
    <script src="../vendor/jquery/jquery.min.js"></script>
    <!-- Bootstrap Core JavaScript -->
    <script src="../vendor/bootstrap/js/bootstrap.min.js"></script>
</body>
</html>
