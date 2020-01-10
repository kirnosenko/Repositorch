import React from 'react';
import { Redirect } from 'react-router-dom'

export default function CreateProject() {
    const [state, setState] = React.useState({
        name: "",
        repositoryPath: "",
        branch: "master",
        useExtendedLog: true,
        checkResult: "1"
    });

    function handleChange(evt) {
        const value =
            evt.target.type === "checkbox" ? evt.target.checked : evt.target.value;
        setState({
            ...state,
            [evt.target.name]: value
        });
    }

    function handleSubmit(event) {
        event.preventDefault();
        
        fetch("api/Projects/Create", {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(state)
        })
        .then((response) => {
            if (!response.ok) throw new Error(response.status);
            setState(null);
        })
        .catch((error) => {
            alert(error);
        });
    }

    if (state == null) {
        return <Redirect to='/' />
    }

    return (
        <div>
            <form onSubmit={handleSubmit}>
                <label>
                    <div className="heading">Project name</div>
                    <input
                        type="text"
                        name="name"
                        value={state.name}
                        onChange={handleChange}
                    />
                </label>
                <br/>
                <label>
                    <div className="heading">Repository path</div>
                    <input
                        type="text"
                        name="repositoryPath"
                        value={state.repositoryPath}
                        onChange={handleChange}
                    />
                </label>
                <br />
                <label>
                    <div className="heading">Repository branch</div>
                    <input
                        type="text"
                        name="branch"
                        value={state.branch}
                        onChange={handleChange}
                    />
                </label>
                <br />
                <label>
                    <input
                        type="checkbox"
                        name="useExtendedLog"
                        checked={state.useExtendedLog}
                        onChange={handleChange}
                    />
                    <div className="heading">Use extended log</div>
                </label>
                <br />
                <label>
                    Check result
                    <select name="checkResult" onChange={handleChange} value={state.checkResult}>
                        <option value="0">Nothing</option>
                        <option value="1">Modified</option>
                        <option value="2">Everything</option>
                    </select>
                </label>
                <br/>
                <button type='submit'>Create project...</button>
            </form>
        </div>
    );
}
