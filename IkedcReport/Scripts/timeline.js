jQuery.noConflict();

(function($,sr){
  var debounce = function (func, threshold, execAsap) {
    var timeout;

    return function debounced () {
        var obj = this, args = arguments;
        function delayed () {
            if (!execAsap)
                func.apply(obj, args);
            timeout = null; 
        };

        if (timeout)
            clearTimeout(timeout);
        else if (execAsap)
            func.apply(obj, args);

        timeout = setTimeout(delayed, threshold || 100); 
    };
  }
  // smartresize 
  jQuery.fn[sr] = function(fn){  return fn ? this.bind('resize', debounce(fn)) : this.trigger(sr); };
})(jQuery,'smartresize');

jQuery(function($) {

  function timeline_itemsize() {
    var container = $('.timeline').width()
    var splitcontainer = container / 2
    var finalwidth = splitcontainer - 104
    $('.timeline_item').css('width', finalwidth + 'px')
  }
  timeline_itemsize();

  $('.timeline').masonry({itemSelector : '.timeline_item'});

  function Arrow_Points() { 
    var s = $('.timeline').find('.timeline_item');
    $.each(s,function(i,obj){
      var posLeft = $(obj).css("left");
      $(obj).addClass('borderclass');
      if(posLeft == "0px") {
        $(this).addClass('left_element');
      } else {
        $(this).addClass('right_element');
      }
    });
  }
  Arrow_Points();

  

  $(window).smartresize(function(){
    timeline_itemsize();
    Arrow_Points();

    $('.timeline').masonry({itemSelector : '.timeline_item'});
  });

});