import React, { Fragment } from 'react';
import {
	LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import Moment from 'moment';
import MultiMetric from '../../MultiMetric';
import SummaryForm from './SummaryForm';

export default function Summary(props) {

	const [data, setData] = React.useState(null);
	const [formData, setFormData] = React.useState(null);
	
	function formatDate(date) {
		return Moment(date).format('YYYY-MM-DD');
	}

	function updateSettings(settings) {
		setFormData({
			...formData,
			settings: settings
		});
		setData(null);
	}

	function renderData(data) {

		function LocLine(title, dataKey, color) {
			return (
				formData.settings[dataKey] ?
					<Line
						type="monotone"
						name={title}
						dataKey={dataKey}
						stroke={color}
						dot={{ r: 0 }} /> : ""
			);
		}

		return (
			<Fragment>
				<SummaryForm
					data={formData}
					useData={s => updateSettings(s)} />
				<ResponsiveContainer aspect={2} >
					<LineChart data={data}>
						<CartesianGrid strokeDasharray="3 3" />
						<XAxis dataKey="date" tickFormatter={formatDate} />
						<YAxis />
						<Tooltip />
						<Legend />
						{LocLine("LOC total", "locTotal", "#0000FF")}
						{LocLine("LOC added", "locAdded", "#00FF00")}
						{LocLine("LOC removed", "locRemoved", "#FF0000")}
					</LineChart>
				</ResponsiveContainer>
			</Fragment>
		);
	}

	return (
		<MultiMetric
			title="Lines Of Code Summary"
			projectMetricPath={props.projectMetricPath}
			renderData={renderData}
			getData={() => data}
			setData={setData}
			getSettings={() => formData !== null ? formData.settings : null}
			setFormData={setFormData} />
	);
}
