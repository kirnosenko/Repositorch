import Moment from 'moment';

export const secondsToDate = (seconds) => {
	return Moment.unix(seconds).toDate();
}

export const secondsToDateFormat = (seconds) => {
	return Moment.unix(seconds).format('YYYY-MM-DD');
}

export const dateToSeconds = (date) => {
	return Moment(date).unix();
}

export const updateObject = (dest, src) => {
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
