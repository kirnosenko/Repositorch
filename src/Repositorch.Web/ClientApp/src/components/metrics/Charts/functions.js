import Moment from 'moment';

export const getColors = (number) => {
	return [
		"#0000FF", "#00FF00", "#FF0000"
	];
}

export const formatDate = (date) => {
	return Moment(date).format('YYYY-MM-DD');
}
