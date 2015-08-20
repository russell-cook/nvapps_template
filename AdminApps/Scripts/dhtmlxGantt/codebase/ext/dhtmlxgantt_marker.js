/*
@license

dhtmlxGantt v.3.3.0 Professional Evaluation
This software is covered by DHTMLX Evaluation License. Contact sales@dhtmlx.com to get Commercial or Enterprise license. Usage without proper license is prohibited.

(c) Dinamenta, UAB.
*/
Gantt.plugin(function(t){t._markers||(t._markers={}),t.config.show_markers=!0,t.attachEvent("onClear",function(){t._markers={}}),t.attachEvent("onGanttReady",function(){function e(e){if(!t.config.show_markers)return!1;if(!e.start_date)return!1;var n=t.getState();if(!(+e.start_date>+n.max_date||+e.end_date&&+e.end_date<+n.min_date||+e.start_date<+n.min_date)){var a=document.createElement("div");a.setAttribute("marker_id",e.id);var i="gantt_marker";t.templates.marker_class&&(i+=" "+t.templates.marker_class(e)),
e.css&&(i+=" "+e.css),e.title&&(a.title=e.title),a.className=i;var s=t.posFromDate(e.start_date);if(a.style.left=s+"px",a.style.height=Math.max(t._y_from_ind(t._order.length),0)+"px",e.end_date){var r=t.posFromDate(e.end_date);a.style.width=Math.max(r-s,0)+"px"}return e.text&&(a.innerHTML="<div class='gantt_marker_content' >"+e.text+"</div>"),a}}var n=document.createElement("div");n.className="gantt_marker_area",t.$task_data.appendChild(n),t.$marker_area=n,t._markerRenderer=t._task_renderer("markers",e,t.$marker_area,null);
}),t.attachEvent("onDataRender",function(){t.renderMarkers()}),t.getMarker=function(t){return this._markers?this._markers[t]:null},t.addMarker=function(t){return t.id=t.id||dhtmlx.uid(),this._markers[t.id]=t,t.id},t.deleteMarker=function(t){return this._markers&&this._markers[t]?(delete this._markers[t],!0):!1},t.updateMarker=function(t){this._markerRenderer&&this._markerRenderer.render_item(t)},t.renderMarkers=function(){if(!this._markers)return!1;if(!this._markerRenderer)return!1;var t=[];for(var e in this._markers)t.push(this._markers[e]);
return this._markerRenderer.render_items(t),!0}});
//# sourceMappingURL=../sources/ext/dhtmlxgantt_marker.js.map