import React, { Fragment } from 'react';
import {
	LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import Moment from 'moment';
import MultiMetric from '../../MultiMetric';
import SummaryForm from './SummaryForm';

export default function Summary(props) {

	const [data, setData] = React.useState(null);
	
	function formatDate(date) {
		return Moment(date).format('YYYY-MM-DD');
	}

	function updateData(data) {
		setData((prevState, props) => {
			if (prevState === null) {
				return data;
			}
			return {
				...prevState,
				...data
			};
		});
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

		return (
			<Fragment>
				<SummaryForm
					data={data}
					updateData={updateData} />
				<ResponsiveContainer aspect={2} >
					<LineChart data={data.result}>
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
			renderMetric={renderMetric}
			getData={() => data}
			setData={updateData} />
	);
}
