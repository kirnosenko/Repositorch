import React, { Fragment } from 'react';
import {
	LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import { getColors, formatDate } from '../functions';
import Metric from '../../Metric';
import SummaryForm from './SummaryForm';

export default function Summary(props) {

	const [data, setData] = React.useState(null);

	function updateSettings(settings) {
		var settingsDelta = {};
		Object.keys(data.settings).forEach(key => {
			if (data.settings[key] !== settings[key]) {
				settingsDelta[key] = settings[key];
			}
		});
		updateData({
			settings: settingsDelta,
			settingsDelta: settingsDelta,
			result: null
		})
	}

	function updateData(data) {
		setData((prevState, props) => {
			if (prevState === null) {
				return data;
			}
			return updateObject({ ...prevState }, data);
		});
	}

	function updateObject(dest, src) {
		Object.keys(src).forEach(key => {
			var value = src[key];
			if (dest[key] === undefined
				|| dest[key] === null
				|| value === null
				|| typeof value != "object") {
				dest[key] = value
			}
			else {
				updateObject(dest[key], value)
			}
		});
		return dest;
	}

	function renderMetric(data) {	
		function LocLine(title, dataKey, color) {
			return (
				data.settings[dataKey] ?
					<Line
						type="monotone"
						name={title}
						dataKey={dataKey}
						stroke={color}
						dot={{ r: 0 }} /> : ""
			);
		}

		var colors = getColors(3);

		return (
			<Fragment>
				<SummaryForm
					settings={data.settings}
					updateSettings={updateSettings} />
				<ResponsiveContainer aspect={2} >
					<LineChart data={data.result}>
						<CartesianGrid strokeDasharray="3 3" />
						<XAxis dataKey="date" tickFormatter={formatDate} />
						<YAxis />
						<Tooltip />
						<Legend />
						{LocLine("LOC total", "locTotal", colors[0])}
						{LocLine("LOC added", "locAdded", colors[1])}
						{LocLine("LOC removed", "locRemoved", colors[2])}
					</LineChart>
				</ResponsiveContainer>
			</Fragment>
		);
	}

	return (
		<Metric
			title="Lines Of Code Summary"
			projectMetricPath={props.projectMetricPath}
			renderMetric={renderMetric}
			getData={() => data}
			setData={updateData} />
	);
}
