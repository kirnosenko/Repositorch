import React, { Fragment } from 'react';
import {
	AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import Moment from 'moment';
import Metric from '../../Metric';

export default function Summary(props) {

	const colors = [
		"#0000FF", "#00FF00", "#FF0000"
	];

	function formatDate(date) {
		return Moment(date).format('YYYY-MM-DD');
	}

	function renderData(data) {

		function LocLine(title, dataKey, color) {
			return (
				<Area
					type="monotone"
					name={title}
					dataKey={dataKey}
					stackId="1"
					stroke={color}
					fill={color} />
			);
		}

		return (
			<Fragment>
				<ResponsiveContainer aspect={2} >
					<AreaChart data={data.values}>
						<CartesianGrid strokeDasharray="3 3" />
						<XAxis dataKey="date" tickFormatter={formatDate} />
						<YAxis />
						<Tooltip />
						<Legend />
						{data.keys.map((key, index) => {
							return LocLine(key, key, colors[index]);
						})}
					</AreaChart>
				</ResponsiveContainer>
			</Fragment>
		);
	}

	return (
		<Metric
			title="Lines Of Code Remains (Burn Down)"
			projectMetricPath={props.projectMetricPath}
			renderData={renderData} />
	);
}
