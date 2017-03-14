<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReCapture.ascx.cs" Inherits="newtelligence.DasBlog.Web.ReCapture" %>

<script type="text/javascript">
    var onloadCallback = function() {
        grecaptcha.render('ReCapture', {
            'sitekey': '6LclXhgUAAAAAGBl7bVhgos3NF4nGVXsa2VNAXIU',
            'callback' : 'onSubmit'
        });
    };

    function onSubmit(token) {
        document.getElementById("add").submit();
    };
</script>

<script src="https://www.google.com/recaptcha/api.js?onload=onloadCallback&render=explicit" async defer></script>

<div id="ReCapture">
</div>