﻿$(document).ready(function () {
    $(document).ready(function () {
        // BAR CHART
        var barOptions = {
            //Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
            scaleBeginAtZero: true,
            //Boolean - Whether grid lines are shown across the chart
            scaleShowGridLines: true,
            //String - Colour of the grid lines
            scaleGridLineColor: "rgba(0,0,0,.05)",
            //Number - Width of the grid lines
            scaleGridLineWidth: 1,
            //Boolean - If there is a stroke on each bar
            barShowStroke: true,
            //Number - Pixel width of the bar stroke
            barStrokeWidth: 1,
            //Number - Spacing between each of the X value sets
            barValueSpacing: 5,
            //Number - Spacing between data sets within X values
            barDatasetSpacing: 1,
            //Boolean - Re-draw chart on page resize
            responsive: true,
            //String - A legend template
            legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%> <li><span style=\"background-color:<%=datasets[i].lineColor%>\"></span> <%if(datasets[i].label){%><%=datasets[i].label%><%}%></li> <%} %></ul > "
        }

        var barData = {};
        $.ajax({
            url: "/Budget/BudgetBarChart",
            type: "GET",
            success: (data, textStatus, jqXHR) => {
                debugger;
                 barData = {
                     labels: data.Labels,
                    datasets: [
                        {
                            label: "My First dataset",
                            fillColor: "rgba(220,220,220,0.5)",
                            strokeColor: "rgba(220,220,220,0.8)",
                            highlightFill: "rgba(220,220,220,0.75)",
                            highlightStroke: "rgba(220,220,220,1)",
                            data: data.BudgetBar
                        },
                        {
                            label: "My Second dataset",
                            fillColor: "rgba(151,187,205,0.5)",
                            strokeColor: "rgba(151,187,205,0.8)",
                            highlightFill: "rgba(151,187,205,0.75)",
                            highlightStroke: "rgba(151,187,205,1)",
                            data: data.SalesBar
                        }
                    ]
                };
                // render chart
                var ctx = document.getElementById("barChart").getContext("2d");
                var myNewChart = new Chart(ctx).Bar(barData, barOptions);
            },
            error: (jqXHR, textStatus, errorThrown) => {
            }
        })



 

        // END BAR CHART
    })
})