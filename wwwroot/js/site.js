// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// jQuery UI easeInOutExpo function
jQuery.extend(jQuery.easing, {
    easeInOutExpo: function (x, t, b, c, d) {
        if (t==0) return b;
        if (t==d) return b+c;
        if ((t/=d/2) < 1) return c/2 * Math.pow(2, 10 * (t - 1)) + b;
        return c/2 * (-Math.pow(2, -10 * --t) + 2) + b;
    }
});

// 侧边栏折叠菜单点击事件
$(document).ready(function() {
    // 为侧边栏的折叠菜单项添加点击事件
    $('.sidebar .nav-item a[data-bs-toggle="collapse"]').on('click', function(e) {
        e.preventDefault();
        var target = $(this).attr('data-bs-target');
        $(target).collapse('toggle');
    });
});
