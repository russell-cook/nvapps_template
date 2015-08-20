/*
@license

dhtmlxGantt v.3.3.0 Professional Evaluation
This software is covered by DHTMLX Evaluation License. Contact sales@dhtmlx.com to get Commercial or Enterprise license. Usage without proper license is prohibited.

(c) Dinamenta, UAB.
*/
Gantt.plugin(function(t){t.config.quickinfo_buttons=["icon_delete","icon_edit"],t.config.quick_info_detached=!0,t.config.show_quick_info=!0,t.attachEvent("onTaskClick",function(e){return t.showQuickInfo(e),!0}),function(){for(var e=["onEmptyClick","onViewChange","onLightbox","onBeforeTaskDelete","onBeforeDrag"],n=function(){return t._hideQuickInfo(),!0},i=0;i<e.length;i++)t.attachEvent(e[i],n)}(),t.templates.quick_info_title=function(t,e,n){return n.text.substr(0,50)},t.templates.quick_info_content=function(t,e,n){
return n.details||n.text},t.templates.quick_info_date=function(e,n,i){return t.templates.task_time(e,n,i)},t.templates.quick_info_class=function(t,e,n){return""},t.showQuickInfo=function(e){if(e!=this._quick_info_box_id&&this.config.show_quick_info){this.hideQuickInfo(!0);var n=this._get_event_counter_part(e);n&&(this._quick_info_box=this._init_quick_info(n,e),this._quick_info_box.className=t._prepare_quick_info_classname(e),this._fill_quick_data(e),this._show_quick_info(n))}},t._hideQuickInfo=function(){
t.hideQuickInfo()},t.hideQuickInfo=function(e){var n=this._quick_info_box;if(this._quick_info_box_id=0,n&&n.parentNode){if(t.config.quick_info_detached)return n.parentNode.removeChild(n);n.className+=" gantt_qi_hidden","auto"==n.style.right?n.style.left="-350px":n.style.right="-350px",e&&n.parentNode.removeChild(n)}},dhtmlxEvent(window,"keydown",function(e){27==e.keyCode&&t.hideQuickInfo()}),t._show_quick_info=function(e){var n=t._quick_info_box;if(t.config.quick_info_detached){n.parentNode&&"#document-fragment"!=n.parentNode.nodeName.toLowerCase()||t.$task_data.appendChild(n);
var i=n.offsetWidth,a=n.offsetHeight,s=this.getScrollState(),r=this.$task.offsetWidth+s.x-i;n.style.left=Math.min(Math.max(s.x,e.left-e.dx*(i-e.width)),r)+"px",n.style.top=e.top-(e.dy?a:-e.height)-25+"px"}else n.style.top="20px",1==e.dx?(n.style.right="auto",n.style.left="-300px",setTimeout(function(){n.style.left="-10px"},1)):(n.style.left="auto",n.style.right="-300px",setTimeout(function(){n.style.right="-10px"},1)),n.className+=" gantt_qi_"+(1==e.dx?"left":"right"),t._obj.appendChild(n)},t._prepare_quick_info_classname=function(e){
var n=t.getTask(e),i="gantt_cal_quick_info",a=this.templates.quick_info_class(n.start_date,n.end_date,n);return a&&(i+=" "+a),i},t._init_quick_info=function(e,n){var i=t.getTask(n);if("boolean"==typeof this._quick_info_readonly&&this._is_readonly(i)!==this._quick_info_readonly&&(t.hideQuickInfo(!0),this._quick_info_box=null),this._quick_info_readonly=this._is_readonly(i),!this._quick_info_box){var a=this._quick_info_box=document.createElement("div"),s='<div class="gantt_cal_qi_title"><div class="gantt_cal_qi_tcontent"></div><div  class="gantt_cal_qi_tdate"></div></div><div class="gantt_cal_qi_content"></div>';
s+='<div class="gantt_cal_qi_controls">';for(var r=t.config.quickinfo_buttons,o={icon_delete:!0,icon_edit:!0},l=0;l<r.length;l++)this._quick_info_readonly&&o[r[l]]||(s+='<div class="gantt_qi_big_icon '+r[l]+'" title="'+t.locale.labels[r[l]]+"\"><div class='gantt_menu_icon "+r[l]+"'></div><div>"+t.locale.labels[r[l]]+"</div></div>");s+="</div>",a.innerHTML=s,dhtmlxEvent(a,"click",function(e){e=e||event,t._qi_button_click(e.target||e.srcElement)}),t.config.quick_info_detached&&dhtmlxEvent(t.$task_data,"scroll",function(){
t.hideQuickInfo()})}return this._quick_info_box},t._qi_button_click=function(e){var n=t._quick_info_box;if(e&&e!=n){var i=e.className;if(-1!=i.indexOf("_icon")){var a=t._quick_info_box_id;t.$click.buttons[i.split(" ")[1].replace("icon_","")](a)}else t._qi_button_click(e.parentNode)}},t._get_event_counter_part=function(e){for(var n=t.getTaskNode(e),i=0,a=0,s=n;s&&"gantt_task"!=s.className;)i+=s.offsetLeft,a+=s.offsetTop,s=s.offsetParent;var r=this.getScrollState();if(s){var o=i+n.offsetWidth/2-r.x>t._x/2?1:0,l=a+n.offsetHeight/2-r.y>t._y/2?1:0;
return{left:i,top:a,dx:o,dy:l,width:n.offsetWidth,height:n.offsetHeight}}return 0},t._fill_quick_data=function(e){var n=t.getTask(e),i=t._quick_info_box;t._quick_info_box_id=e;var a=i.firstChild.firstChild;a.innerHTML=t.templates.quick_info_title(n.start_date,n.end_date,n);var s=a.nextSibling;s.innerHTML=t.templates.quick_info_date(n.start_date,n.end_date,n);var r=i.firstChild.nextSibling;r.innerHTML=t.templates.quick_info_content(n.start_date,n.end_date,n)}});
//# sourceMappingURL=../sources/ext/dhtmlxgantt_quick_info.js.map