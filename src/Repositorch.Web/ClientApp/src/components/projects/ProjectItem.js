import React from 'react'
import * as signalR from '@aspnet/signalr';
import { useSelector, useDispatch } from 'react-redux';
import { addMapping, updateMapping, removeMapping } from '../../state/actions';

const styles = {
    li: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: '.5rem 1rem',
        border: '1px solid #ccc',
        borderRadius: '4px',
        marginBottom: '.5rem'
    }
}

export default function ProjectItem(props) {
    const mapping = useSelector(state => state.mappings[props.name]);
    const progress = mapping !== undefined && mapping.done !== undefined
        ? mapping.done + '/' + mapping.total
        : '';
    const dispatch = useDispatch();

    function openConnection() {
        if (mapping !== undefined) return;

        var connection = new signalR.HubConnectionBuilder()
            .withUrl('/Hubs/Mapping').build();
        connection.start()
            .then(_ => {
                connection.on('Progress', (done, total, error, working) => {
                    dispatch(updateMapping(props.name, done, total, error, working));
                });
                connection.invoke('WatchProject', props.name);
            });
        dispatch(addMapping(props.name, connection));
    }

    function closeConnection() {
        if (mapping === undefined) return;

        mapping.connection.off('Progress');
        mapping.connection.stop();
        dispatch(removeMapping(props.name));
    }

    function startMapping() {
        fetch('api/Mapping/Start/' + props.name, {
            method: 'PUT'
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            openConnection();
        })
        .catch((e) => {
            console.error(e);
        });
    }

    function stopMapping() {
        fetch('api/Mapping/Stop/' + props.name, {
            method: 'PUT'
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            closeConnection();
        })
        .catch((e) => {
            console.error(e);
        });
    }

    function switchMapping() {
        mapping === undefined
            ? startMapping()
            : stopMapping();
    }

    function removeProject() {
        closeConnection();
        props.removeProject(props.name);
    }

    React.useEffect(() => {
    }, [mapping]);

    return (
        <li style={styles.li}>
            <span>
                {props.name}
            </span>
            <span>
                {progress}
            </span>
            <button
                type="button"
                className="btn btn-primary btn-sm"
                onClick={() => switchMapping()}>
                {mapping === undefined ? "Start mapping..." : "Stop mapping"}
            </button>
            <button
                type="button"
                className="btn btn-danger btn-sm"
                onClick={() => removeProject()}>Remove</button>
        </li>
    )
}
