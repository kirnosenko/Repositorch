import Moment from 'moment';

export const getColors = (number) => {
	var masks = [
		"#0000X",
		"#00X00",
		"#X0000",
		"#00XX",
		"#X00X",
		"#XX00",
	];
	var colors = [];
	var counter = 0;
	var chanel = 256;
	var mask = 0;

	while (counter < number) {
		var hex = (chanel - 1).toString(16);
		var color = masks[mask].replace(/X/g, hex);
		colors.push(color);
		counter++;
		mask++;
		if (mask === masks.length) {
			mask = 0;
			chanel = chanel >>> 1;
		}
	}

	return colors;
}

export const formatDate = (date) => {
	return Moment(date).format('YYYY-MM-DD');
}
