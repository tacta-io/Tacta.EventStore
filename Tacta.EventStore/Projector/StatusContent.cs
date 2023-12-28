namespace Tacta.EventStore.Projector
{
    internal static class StatusContent
    {
        public static string Html = @"
            <html>
            <head>
                <meta http-equiv='refresh' , content='{refresh}' />
                <title>{service} Projections Monitoring</title>
                <script src='https://cdn.syncfusion.com/ej2/dist/ej2.min.js' type='text/javascript'></script>
            </head>

            <body style='padding: 100'>

            <div id='container'></div>

            <script>
	                    var lastEventSequence = {sequence};
		                var data = [{data}];

		                data.forEach(x => x.color = x.sequence == lastEventSequence ? '#99c4ff' : '#ff99b0');

		                var chart = new ej.charts.Chart({
			                title: '{service} Projections Monitoring',
			                primaryXAxis: { valueType: 'Category' },
				            primaryYAxis: { title: 'Event Sequence ({sequence})' },
			                series: [{
				                marker: { dataLabel: { visible: true } },
				                type: 'Bar',
				                dataSource: data,
				                xName: 'projection',
				                yName: 'sequence',
					            pointColorMapping: 'color',
                                animation: { enable: false }
			                }]
			            });

		                chart.appendTo('#container');
                </script>

            </body>
            </html>";
    }
}
