import React, { Fragment } from 'react';
import DatePicker from "react-datepicker";
import propTypes from 'prop-types';
import { secondsToDate, dateToSeconds } from '../functions';
import "react-datepicker/dist/react-datepicker.css";

export default function FromTillDatesPicker(props) {
	return (
		<Fragment>
			<div className="form-group">
				<div className="heading">From date</div>
				<DatePicker
					selected={secondsToDate(props.settings.dateFrom)}
					onChange={(date) => props.setSetting("dateFrom", dateToSeconds(date))} />
			</div>
			<div className="form-group">
				<div className="heading">Till date</div>
				<DatePicker
					selected={secondsToDate(props.settings.dateTo)}
					onChange={(date) => props.setSetting("dateTo", dateToSeconds(date))} />
			</div>
		</Fragment>
	);
}

FromTillDatesPicker.propTypes = {
	settings: propTypes.object.isRequired,
	setSetting: propTypes.func.isRequired
};
