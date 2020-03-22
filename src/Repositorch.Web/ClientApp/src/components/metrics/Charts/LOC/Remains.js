import React, { Fragment } from 'react';
import {
	AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import { getColors, formatDate } from '../functions';
import Metric from '../../Metric';

export default function Summary(props) {

	function renderResult(result) {

		function LocLine(title, dataKey, color) {
			return (
				<Area
					type="monotone"
					name={title}
					key={dataKey}
					dataKey={dataKey}
					stackId="1"
					stroke={color}
					fill={color} />
			);
		}

		var colors = getColors(result.keys.lenght);

		return (
			<Fragment>
				<ResponsiveContainer aspect={2} >
					<AreaChart data={result.values}>
						<CartesianGrid strokeDasharray="3 3" />
						<XAxis dataKey="date" tickFormatter={formatDate} />
						<YAxis />
						<Tooltip />
						<Legend />
						{result.keys.map((key, index) => {
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
			renderResult={renderResult} />
	);
}
