import React from 'react';
import { ContentToLoad } from '../ContentToLoad';

export default function Metric(props) {
    return (
        <div>
            <h2>{props.title}</h2>
            <ContentToLoad
                url={'api/Metrics/Calculate/'.concat(props.metric)}
                renderData={props.renderData} />
        </div>
    );
}
