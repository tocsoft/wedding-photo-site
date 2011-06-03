/*
* jQuery hashchange event - v1.3 - 7/21/2010
* http://benalman.com/projects/jquery-hashchange-plugin/
* 
* Copyright (c) 2010 "Cowboy" Ben Alman
* Dual licensed under the MIT and GPL licenses.
* http://benalman.com/about/license/
*/
(function ($, e, b) { var c = "hashchange", h = document, f, g = $.event.special, i = h.documentMode, d = "on" + c in e && (i === b || i > 7); function a(j) { j = j || location.href; return "#" + j.replace(/^[^#]*#?(.*)$/, "$1") } $.fn[c] = function (j) { return j ? this.bind(c, j) : this.trigger(c) }; $.fn[c].delay = 50; g[c] = $.extend(g[c], { setup: function () { if (d) { return false } $(f.start) }, teardown: function () { if (d) { return false } $(f.stop) } }); f = (function () { var j = {}, p, m = a(), k = function (q) { return q }, l = k, o = k; j.start = function () { p || n() }; j.stop = function () { p && clearTimeout(p); p = b }; function n() { var r = a(), q = o(m); if (r !== m) { l(m = r, q); $(e).trigger(c) } else { if (q !== m) { location.href = location.href.replace(/#.*/, "") + q } } p = setTimeout(n, $.fn[c].delay) } $.browser.msie && !d && (function () { var q, r; j.start = function () { if (!q) { r = $.fn[c].src; r = r && r + a(); q = $('<iframe tabindex="-1" title="empty"/>').hide().one("load", function () { r || l(a()); n() }).attr("src", r || "javascript:0").insertAfter("body")[0].contentWindow; h.onpropertychange = function () { try { if (event.propertyName === "title") { q.document.title = h.title } } catch (s) { } } } }; j.stop = k; o = function () { return a(q.location.href) }; l = function (v, s) { var u = q.document, t = $.fn[c].domain; if (v !== s) { u.title = h.title; u.open(); t && u.write('<script>document.domain="' + t + '"<\/script>'); u.close(); q.location.hash = v } } })(); return j })() })(jQuery, this);


$(function () {

    //the loading image
    var $loader = $('#st_loading');
    //the ul element 
    var $list = $('#st_nav');


    //the current image being shown
    var $currImage = $('#st_main').children('img:first');



    var skipHashChange = false;
    function imageChanged() {
        if (!skipHashChange) {
            var hash = window.location.hash.replace('!', '');
            if (hash.length > 1) {
                var tmp = $(hash);
                if (tmp.length > 0) {
                    tmp.click();
                } else
                    $list.find('img.default').click();
            }

            else
                $list.find('img.default').click();
        }
    }

    $(window).hashchange(imageChanged);

    //let's load the current image 
    //and just then display the navigation menu
    $('<img>').load(function () {
        $loader.hide();
        $currImage.fadeIn(3000);
        //slide out the menu
        setTimeout(function () {
            $list.animate({ 'left': '0px' }, 500);
        },
					1000);
    }).attr('src', $currImage.attr('src'));

    //calculates the width of the div element 
    //where the thumbs are going to be displayed
    buildThumbs();

    function buildThumbs() {
        $list.children('li.album').each(function () {
            var $elem = $(this);
            var $thumbs_wrapper = $elem.find('.st_thumbs_wrapper');
            var $thumbs = $thumbs_wrapper.children(':first');
            //each thumb has 180px and we add 3 of margin
            var finalW = $thumbs.find('img').length * 183;
            $thumbs.css('width', finalW + 'px');
            //make this element scrollable
            makeScrollable($thumbs_wrapper, $thumbs);
        });
    }


    //clicking on the menu items (up and down arrow)
    //makes the thumbs div appear, and hides the current 
    //opened menu (if any)
    $list.find('.st_arrow_down').live('click', function (e) {
        var $this = $(this);

        hideThumbs();


        $this.addClass('st_arrow_up').removeClass('st_arrow_down');
        var $elem = $this.closest('li');

        $('img', $elem).each(function () {
            $this = $(this);
            var src = $this.attr('src', $this.attr('data-src'));
        });

        $elem.find('.st_wrapper').show();
        var $thumbs_wrapper = $elem.find('.st_thumbs_wrapper');
        var $thumbs = $thumbs_wrapper.children(':first');
        //each thumb has 180px and we add 3 of margin
        var finalW = $thumbs.find('img').length * 183;

        $thumbs.css('width', finalW + 'px');
        //make this element scrollable
        makeScrollable($thumbs_wrapper, $thumbs);

        $elem.addClass('current').animate({ 'height': '150px' }, 200);
        var $thumbs_wrapper = $this.parent().next();
        $thumbs_wrapper.show(200);
        e.preventDefault();
        return false;
    });
    $list.find('.st_arrow_up').live('click', function (e) {
        var $this = $(this);
        $this.addClass('st_arrow_down').removeClass('st_arrow_up');
        hideThumbs();
        e.preventDefault();
        return false;
    });

    //clicking on a thumb, replaces the large image
    $list.find('.st_thumbs img').bind('click', function (e) {

        var $this = $(this);
        skipHashChange = true;
        window.location.hash = '!' + $this.attr('id');

        $loader.show();
        $('<img class="st_preview"/>').load(function () {
            var $this = $(this);
            var $currImage = $('#st_main').children('img:first');
            $this.insertBefore($currImage);
            $loader.hide();

            $currImage.remove();

            /*
            $currImage.stop().fadeOut(2000, function () {
            $(this).remove();
            });
            */
            skipHashChange = false;
        }).attr('src', $this.attr('href'));

        $('h2').text($this.attr('title'));
        attribUrl = $this.attr('data-attribution-url');
        var atrib = $('#attribution').text($this.attr('data-attribution'));
        if (attribUrl)
            atrib.attr('href', attribUrl);
        else
            atrib.removeAttr('href');

        e.preventDefault();
        return false;
    }).bind('mouseenter', function () {
        $(this).stop().animate({ 'opacity': '1' });
    }).bind('mouseleave', function () {
        $(this).stop().animate({ 'opacity': '0.7' });
    });

    //function to hide the current opened menu
    function hideThumbs(callback) {
        var cb = callback;
        $list.find('li.current')
						 .animate({ 'height': '40px' }, 400, function () {
						     $(this).removeClass('current');
						     if (cb)
						         cb();
						 })
						 .find('.st_thumbs_wrapper')
						 .hide(200)
						 .andSelf()
						 .find('.st_link span')
						 .addClass('st_arrow_down')
						 .removeClass('st_arrow_up');
    }

    //makes the thumbs div scrollable
    //on mouse move the div scrolls automatically
    function makeScrollable($outer, $inner) {
        var extra = 800;
        //Get menu width
        var divWidth = $outer.width();
        //Remove scrollbars
        $outer.css({
            overflow: 'hidden'
        });
        //Find last image in container
        var lastElem = $inner.find('img:last');
        $outer.scrollLeft(0);
        //When user move mouse over menu
        $outer.unbind('mousemove').bind('mousemove', function (e) {
            var containerWidth = lastElem[0].offsetLeft + lastElem.outerWidth() + 2 * extra;
            var left = (e.pageX - $outer.offset().left) * (containerWidth - divWidth) / divWidth - extra;
            $outer.scrollLeft(left);
        });
    }

    var autoFirstHideTimer;

    function hideAlbums() {
        clearTimeout(autoFirstHideTimer);
        $('.st_link').each(function () {
            var st = $(this);
            st.stop();

            if (st.siblings(':visible').length == 0)
                st.animate({ left: -(st.width() - 15) });
        });
    }

    $('body').live('click', function () {
        hideThumbs(hideAlbums);
    });
    function showAlbum(st) {
        var st = $(st);
        st.stop();
        st.animate({ left: 0 }, function () {

        });
    }

    $('.st_link').mouseout(function () {
        hideAlbums();
    });

    $('.st_link').mouseover(function () {
        showAlbum(this)
    });

    autoFirstHideTimer = setTimeout(hideAlbums, 5000);
    imageChanged();

});