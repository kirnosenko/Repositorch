import React, { Fragment } from 'react';
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import Moment from 'moment';
import Metric from '../Metric';

function formatDate(date) {
  return Moment(date).format('YYYY-MM-DD');
}

function renderData(data) {
  return (
    <LineChart
      width={600}
      height={400}
      data={data}
      margin={{top: 5, right: 30, left: 20, bottom: 5}}>
      <CartesianGrid strokeDasharray="3 3" />
      <XAxis dataKey="date" tickFormatter={formatDate} />
      <YAxis />
      <Tooltip />
      <Legend />
      <Line type="monotone" dataKey="loc" stroke="#8884d8" dot={{r: 0}} />
    </LineChart>
	);
}

export default function Loc(props) {
  return (
    <Metric
			title="Lines Of Code"
			projectMetricPath={props.projectMetricPath}
			renderData={renderData} />
	);
}
