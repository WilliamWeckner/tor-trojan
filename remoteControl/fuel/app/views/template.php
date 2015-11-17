<!doctype html>
<html class="no-js" lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>remoteControl</title>
    <?=Asset::css('foundation.css')?>
    <?=Asset::js('modernizr.js')?>
</head>
<body>

<? if(Session::get('user')):?>
    <nav class="top-bar" data-topbar>
        <ul class="title-area">
            <li class="name">
                <h1><a href="<?=Uri::create('panel')?>">remoteControl</a></h1>
            </li>
            <li class="toggle-topbar menu-icon">
                <a href="#"><span>Menu</span></a>
            </li>
        </ul>
        <section class="top-bar-section">
            <!-- Left Nav Section -->
            <ul class="left">
                <li><a href="<?=Uri::create('clients')?>">Clients</a></li>
            </ul>

            <!-- Right Nav Section -->
            <ul class="right">
                <li class="has-form">
                    <a href="<?=Uri::create('panel/logout')?>" class="button alert">Logout</a>
                </li>
            </ul>
        </section>
    </nav>
<? endif;?>

<div style="margin-top: 25px;"></div>

<div class="row">
    <div class="medium-8 small-centered columns">
        <?=$content?>
    </div>
</div>

<?=Asset::js('jquery.js')?>
<?=Asset::js('foundation.min.js')?>
<script>
    $(document).foundation();
</script>
</body>
</html>
