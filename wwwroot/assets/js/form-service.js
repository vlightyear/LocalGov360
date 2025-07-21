let formConfig = {
    id: null,
    serviceName: '',
    serviceDescription: '',
    createdAt: '',
    pages: [
        {
            id: 1,
            name: 'Page 1',
            fields: []
        }
    ]
};

let currentPageId = 1;
let fieldCounter = 0;
let savedForms = JSON.parse(localStorage.getItem('savedForms')) || [];
let currentPageView = 'builder';
let currentSubmissionPage = 0; // Track current page in view mode

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    loadServices();
    renderPageNavigation();
    renderPreview();
});

// Page Navigation
function showPage(page) {
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.getElementById(page + 'Page').classList.add('active');
    currentPageView = page;

    if (page === 'services') {
        loadServices();
    } else if (page === 'builder') {
        if (!formConfig.id) {
            resetForm();
        }
    }
}

// Add Field Functionality
function addField() {
    const currentPage = formConfig.pages.find(p => p.id === currentPageId);
    if (!currentPage) return;

    const container = document.getElementById('formFieldsContainer');
    const fieldId = ++fieldCounter;

    const fieldHtml = `
        <div class="input-item" id="field-${fieldId}">
            <button class="remove-btn" onclick="removeField(${fieldId})" title="Remove field">×</button>
            
            <div class="field-header">Field ${fieldId}</div>
            
            <div class="form-group">
                <label for="fieldType-${fieldId}">Field Type</label>
                <select id="fieldType-${fieldId}" class="form-control" onchange="updatePreview()">
                    <option value="text">Text Input</option>
                    <option value="textarea">Text Area</option>
                    <option value="file">File Upload</option>
                    <option value="email">Email</option>
                    <option value="number">Number</option>
                    <option value="date">Date</option>
                    <option value="select">Dropdown</option>
                    <option value="checkbox">Checkbox</option>
                    <option value="radio">Radio Buttons</option>
                </select>
            </div>
            
            <div class="form-group">
                <label for="fieldLabel-${fieldId}">Field Label</label>
                <input type="text" id="fieldLabel-${fieldId}" class="form-control" placeholder="Enter field label" oninput="updatePreview()">
            </div>
            
            <div class="form-group">
                <label for="fieldPlaceholder-${fieldId}">Placeholder Text</label>
                <input type="text" id="fieldPlaceholder-${fieldId}" class="form-control" placeholder="Enter placeholder text" oninput="updatePreview()">
            </div>
            
            <div class="form-group" id="selectOptions-${fieldId}" style="display: none;">
                <label for="fieldOptions-${fieldId}">Options (one per line)</label>
                <textarea id="fieldOptions-${fieldId}" class="form-control" rows="3" placeholder="Option 1\nOption 2\nOption 3" oninput="updatePreview()"></textarea>
            </div>
            
            <div class="form-group">
                <label style="display: flex; align-items: center; gap: 0.5rem;">
                    <input type="checkbox" id="fieldRequired-${fieldId}" onchange="updatePreview()">
                    Required field
                </label>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', fieldHtml);

    const fieldTypeSelect = document.getElementById(`fieldType-${fieldId}`);
    fieldTypeSelect.addEventListener('change', function () {
        toggleSelectOptions(fieldId);
        updatePreview();
    });

    updatePreview();
}

function toggleSelectOptions(fieldId) {
    const fieldType = document.getElementById(`fieldType-${fieldId}`).value;
    const optionsDiv = document.getElementById(`selectOptions-${fieldId}`);

    if (['select', 'radio', 'checkbox'].includes(fieldType)) {
        optionsDiv.style.display = 'block';
    } else {
        optionsDiv.style.display = 'none';
    }
}

function removeField(fieldId) {
    const fieldElement = document.getElementById(`field-${fieldId}`);
    if (fieldElement) {
        fieldElement.remove();
        updatePreview();
    }
}

function updatePreview() {
    collectFormData();
    renderPreview();
    renderPageNavigation();
}

function collectFormData() {
    formConfig.serviceName = document.getElementById('serviceName').value;
    formConfig.serviceDescription = document.getElementById('serviceDescription').value;

    const currentPage = formConfig.pages.find(p => p.id === currentPageId);
    if (!currentPage) return;

    currentPage.fields = [];

    const fieldElements = document.querySelectorAll('#formFieldsContainer .input-item');

    fieldElements.forEach(fieldElement => {
        const fieldId = fieldElement.id.split('-')[1];

        const fieldType = document.getElementById(`fieldType-${fieldId}`);
        const fieldLabel = document.getElementById(`fieldLabel-${fieldId}`);
        const fieldPlaceholder = document.getElementById(`fieldPlaceholder-${fieldId}`);
        const fieldRequired = document.getElementById(`fieldRequired-${fieldId}`);
        const fieldOptions = document.getElementById(`fieldOptions-${fieldId}`);

        if (fieldType && fieldLabel && fieldPlaceholder && fieldRequired) {
            const field = {
                id: fieldId,
                type: fieldType.value,
                label: fieldLabel.value || `Field ${fieldId}`,
                placeholder: fieldPlaceholder.value,
                required: fieldRequired.checked,
                options: fieldOptions ? fieldOptions.value.split('\n').filter(opt => opt.trim()) : []
            };

            currentPage.fields.push(field);
        }
    });
}

function saveForm() {
    if (!formConfig.serviceName.trim()) {
        alert('Please enter a service name.');
        return;
    }

    const hasFields = formConfig.pages.some(page => page.fields.length > 0);
    if (!hasFields) {
        alert('Please add at least one field to any page.');
        return;
    }

    for (const page of formConfig.pages) {
        const invalidFields = page.fields.filter(field => !field.label.trim());
        if (invalidFields.length > 0) {
            alert('Please provide labels for all fields.');
            return;
        }
    }

    const isNewForm = !formConfig.id;

    if (isNewForm) {
        formConfig.id = Date.now();
        formConfig.createdAt = new Date().toLocaleString();
        savedForms.push({ ...formConfig });
    } else {
        const index = savedForms.findIndex(f => f.id === formConfig.id);
        if (index !== -1) {
            savedForms[index] = { ...formConfig };
        }
    }

    localStorage.setItem('savedForms', JSON.stringify(savedForms));

    const successDiv = document.getElementById('successMessage');
    successDiv.innerHTML = `<div class="success-message">✅ Form ${isNewForm ? 'saved' : 'updated'} successfully!</div>`;
    successDiv.style.display = 'block';

    setTimeout(() => {
        successDiv.style.display = 'none';
    }, 4000);

    document.getElementById('deleteBtn').style.display = 'inline-block';
    loadServices();
}

function clearForm() {
    if (confirm('Are you sure you want to clear all form data?')) {
        resetForm();
    }
}

function resetForm() {
    document.getElementById('serviceName').value = '';
    document.getElementById('serviceDescription').value = '';
    document.getElementById('formFieldsContainer').innerHTML = '';
    document.getElementById('previewContainer').innerHTML = '<div style="text-align: center; color: #666; font-style: italic; padding: 2rem;"><p>Add fields to see the preview</p></div>';
    document.getElementById('deleteBtn').style.display = 'none';

    formConfig = {
        id: null,
        serviceName: '',
        serviceDescription: '',
        createdAt: '',
        pages: [
            {
                id: 1,
                name: 'Page 1',
                fields: []
            }
        ]
    };

    currentPageId = 1;
    fieldCounter = 0;
    renderPageNavigation();
}

function deleteForm() {
    if (!formConfig.id) return;

    if (confirm('Are you sure you want to delete this form? This action cannot be undone.')) {
        savedForms = savedForms.filter(f => f.id !== formConfig.id);
        localStorage.setItem('savedForms', JSON.stringify(savedForms));
        resetForm();
        loadServices();

        const successDiv = document.getElementById('successMessage');
        successDiv.innerHTML = '<div class="success-message">🗑️ Form deleted successfully.</div>';
        successDiv.style.display = 'block';

        setTimeout(() => {
            successDiv.style.display = 'none';
        }, 4000);
    }
}

function renderPreview() {
    const previewContainer = document.getElementById('previewContainer');

    const hasFields = formConfig.pages.some(page => page.fields.length > 0);

    if (!hasFields) {
        previewContainer.innerHTML = '<div style="text-align: center; color: #666; font-style: italic; padding: 2rem;"><p>Add fields to see the preview</p></div>';
        return;
    }

    let previewHtml = `
        <div class="preview-form">
            <h3 style="margin-bottom: 1rem; color: #333;">${formConfig.serviceName || 'Untitled Service'}</h3>
            <p style="color: #666; margin-bottom: 1.5rem;">${formConfig.serviceDescription || 'No description provided.'}</p>
            <hr style="margin-bottom: 1.5rem;">
            <form>
    `;

    const currentPage = formConfig.pages.find(p => p.id === currentPageId);
    if (currentPage) {
        currentPage.fields.forEach(field => {
            previewHtml += `
                <div class="form-group">
                    <label for="preview-${field.id}">${field.label}${field.required ? ' <span class="required-indicator">*</span>' : ''}</label>`;

            switch (field.type) {
                case 'textarea':
                    previewHtml += `<textarea id="preview-${field.id}" class="form-control" placeholder="${field.placeholder}" ${field.required ? 'required' : ''}></textarea>`;
                    break;
                case 'select':
                    previewHtml += `<select id="preview-${field.id}" class="form-control" ${field.required ? 'required' : ''}>
                        <option value="">Choose an option</option>`;
                    field.options.forEach(option => {
                        previewHtml += `<option value="${option}">${option}</option>`;
                    });
                    previewHtml += `</select>`;
                    break;
                case 'radio':
                    field.options.forEach((option, index) => {
                        previewHtml += `
                            <div style="margin-bottom: 0.5rem;">
                                <input type="radio" id="preview-${field.id}-${index}" name="preview-${field.id}" value="${option}" ${field.required ? 'required' : ''}>
                                <label for="preview-${field.id}-${index}" style="margin-left: 0.5rem;">${option}</label>
                            </div>
                        `;
                    });
                    break;
                case 'checkbox':
                    field.options.forEach((option, index) => {
                        previewHtml += `
                            <div style="margin-bottom: 0.5rem;">
                                <input type="checkbox" id="preview-${field.id}-${index}" name="preview-${field.id}[]" value="${option}">
                                <label for="preview-${field.id}-${index}" style="margin-left: 0.5rem;">${option}</label>
                            </div>
                        `;
                    });
                    break;
                default:
                    previewHtml += `<input type="${field.type}" id="preview-${field.id}" class="form-control" placeholder="${field.placeholder}" ${field.required ? 'required' : ''}>`;
            }

            previewHtml += `</div>`;
        });
    }

    if (formConfig.pages.length > 1) {
        previewHtml += `
            <div style="display: flex; justify-content: space-between; margin-top: 2rem;">
                <button type="button" class="btn btn-secondary" ${currentPageId === formConfig.pages[0].id ? 'disabled' : ''} onclick="switchToPreviousPage()">Previous</button>
                <button type="button" class="btn btn-secondary" ${currentPageId === formConfig.pages[formConfig.pages.length - 1].id ? 'disabled' : ''} onclick="switchToNextPage()">Next</button>
            </div>
        `;
    }

    previewHtml += `
                <button type="button" class="btn btn-success" style="margin-top: 1rem;">Submit Form</button>
            </form>
        </div>
    `;

    previewContainer.innerHTML = previewHtml;
}

function switchToPreviousPage() {
    const currentIndex = formConfig.pages.findIndex(p => p.id === currentPageId);
    if (currentIndex > 0) {
        switchPage(formConfig.pages[currentIndex - 1].id);
    }
}

function switchToNextPage() {
    const currentIndex = formConfig.pages.findIndex(p => p.id === currentPageId);
    if (currentIndex < formConfig.pages.length - 1) {
        switchPage(formConfig.pages[currentIndex + 1].id);
    }
}

function renderPageNavigation() {
    const container = document.getElementById('pageNavigation');
    container.innerHTML = '';

    formConfig.pages.forEach((page, index) => {
        const btn = document.createElement('button');
        btn.className = `page-btn ${page.id === currentPageId ? 'active' : ''}`;
        btn.textContent = page.name || `Page ${index + 1}`;
        btn.onclick = () => switchPage(page.id);
        container.appendChild(btn);
    });

    const addBtn = document.createElement('button');
    addBtn.className = 'page-btn add-page-btn';
    addBtn.textContent = '+ Add Page';
    addBtn.onclick = addPage;
    container.appendChild(addBtn);
}

function switchPage(pageId) {
    collectFormData();
    currentPageId = pageId;

    const container = document.getElementById('formFieldsContainer');
    container.innerHTML = '';

    const page = formConfig.pages.find(p => p.id === pageId);
    if (page) {
        fieldCounter = Math.max(fieldCounter, ...page.fields.map(f => parseInt(f.id)) || [0]);

        page.fields.forEach(field => {
            const fieldId = field.id;

            const fieldHtml = `
                <div class="input-item" id="field-${fieldId}">
                    <button class="remove-btn" onclick="removeField(${fieldId})" title="Remove field">×</button>
                    
                    <div class="field-header">Field ${fieldId}</div>
                    
                    <div class="form-group">
                        <label for="fieldType-${fieldId}">Field Type</label>
                        <select id="fieldType-${fieldId}" class="form-control" onchange="updatePreview()">
                            <option value="text" ${field.type === 'text' ? 'selected' : ''}>Text Input</option>
                            <option value="textarea" ${field.type === 'textarea' ? 'selected' : ''}>Text Area</option>
                            <option value="file" ${field.type === 'file' ? 'selected' : ''}>File Upload</option>
                            <option value="email" ${field.type === 'email' ? 'selected' : ''}>Email</option>
                            <option value="number" ${field.type === 'number' ? 'selected' : ''}>Number</option>
                            <option value="date" ${field.type === 'date' ? 'selected' : ''}>Date</option>
                            <option value="select" ${field.type === 'select' ? 'selected' : ''}>Dropdown</option>
                            <option value="checkbox" ${field.type === 'checkbox' ? 'selected' : ''}>Checkbox</option>
                            <option value="radio" ${field.type === 'radio' ? 'selected' : ''}>Radio Buttons</option>
                        </select>
                    </div>
                    
                    <div class="form-group">
                        <label for="fieldLabel-${fieldId}">Field Label</label>
                        <input type="text" id="fieldLabel-${fieldId}" class="form-control" placeholder="Enter field label" value="${field.label}" oninput="updatePreview()">
                    </div>
                    
                    <div class="form-group">
                        <label for="fieldPlaceholder-${fieldId}">Placeholder Text</label>
                        <input type="text" id="fieldPlaceholder-${fieldId}" class="form-control" placeholder="Enter placeholder text" value="${field.placeholder || ''}" oninput="updatePreview()">
                    </div>
                    
                    <div class="form-group" id="selectOptions-${fieldId}" style="display: ${['select', 'radio', 'checkbox'].includes(field.type) ? 'block' : 'none'};">
                        <label for="fieldOptions-${fieldId}">Options (one per line)</label>
                        <textarea id="fieldOptions-${fieldId}" class="form-control" rows="3" placeholder="Option 1\nOption 2\nOption 3" oninput="updatePreview()">${field.options.join('\n')}</textarea>
                    </div>
                    
                    <div class="form-group">
                        <label style="display: flex; align-items: center; gap: 0.5rem;">
                            <input type="checkbox" id="fieldRequired-${fieldId}" ${field.required ? 'checked' : ''} onchange="updatePreview()">
                            Required field
                        </label>
                    </div>
                </div>
            `;

            container.insertAdjacentHTML('beforeend', fieldHtml);

            const fieldTypeSelect = document.getElementById(`fieldType-${fieldId}`);
            fieldTypeSelect.addEventListener('change', function () {
                toggleSelectOptions(fieldId);
                updatePreview();
            });
        });
    }

    updatePreview();
}

function addPage() {
    const newPageId = Date.now();
    formConfig.pages.push({
        id: newPageId,
        name: `Page ${formConfig.pages.length + 1}`,
        fields: []
    });

    switchPage(newPageId);
}

function loadServices() {
    savedForms = JSON.parse(localStorage.getItem('savedForms')) || [];
    const container = document.getElementById('servicesContainer');

    if (savedForms.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <h3>No Services Available</h3>
                <p>Create your first service form to get started</p>
                <button class="btn btn-primary" onclick="showPage('builder')">Create Service</button>
            </div>
        `;
        return;
    }

    let servicesHtml = '<div class="services-grid">';
    savedForms.forEach(form => {
        const totalFields = form.pages.reduce((sum, page) => sum + page.fields.length, 0);

        servicesHtml += `
            <div class="service-card">
                <h3>${form.serviceName}</h3>
                <p>${form.serviceDescription || 'No description provided'}</p>
                <div class="service-meta">
                    <span>Created: ${form.createdAt || new Date(form.id).toLocaleString()}</span>
                    <div style="display: flex; gap: 0.5rem;">
                        <span class="field-count">${totalFields} fields</span>
                        <span class="page-count">${form.pages.length} pages</span>
                    </div>
                </div>
                <div style="margin-top: 1rem; display: flex; gap: 0.5rem;">
                    <button class="btn btn-primary" onclick="editForm(${form.id})">Edit</button>
                    <button class="btn btn-secondary" onclick="openForm(${form.id})">View</button>
                </div>
            </div>
        `;
    });
    servicesHtml += '</div>';

    container.innerHTML = servicesHtml;
}

function editForm(formId) {
    formId = typeof formId === 'string' ? parseInt(formId) : formId;

    const form = savedForms.find(f => f.id === formId);
    if (!form) {
        alert('Form not found!');
        return;
    }

    formConfig = JSON.parse(JSON.stringify(form));

    document.getElementById('serviceName').value = formConfig.serviceName;
    document.getElementById('serviceDescription').value = formConfig.serviceDescription;

    document.getElementById('deleteBtn').style.display = 'inline-block';

    if (formConfig.pages.length > 0) {
        currentPageId = formConfig.pages[0].id;
        switchPage(currentPageId);
    }

    showPage('builder');
}

function openForm(formId) {
    formId = typeof formId === 'string' ? parseInt(formId) : formId;

    const form = savedForms.find(f => f.id === formId);
    if (!form) {
        alert('Form not found!');
        return;
    }

    // Reset to first page when opening form
    currentSubmissionPage = 0;
    renderSubmissionPage(form, currentSubmissionPage);
    showPage('submission');
}

function renderSubmissionPage(form, pageIndex) {
    const page = form.pages[pageIndex];
    let submissionHtml = `
        <div class="submission-header">
            <h2>${form.serviceName}</h2>
            <p>${form.serviceDescription}</p>
        </div>
        <div class="submission-form">
            <form id="submissionForm-${form.id}">
    `;

    if (form.pages.length > 1) {
        submissionHtml += `<h3 style="margin-bottom: 1.5rem;">${page.name || `Page ${pageIndex + 1}`}</h3>`;
    }

    page.fields.forEach(field => {
        submissionHtml += `
            <div class="form-group">
                <label for="submit-${field.id}">${field.label}${field.required ? ' <span class="required-indicator">*</span>' : ''}</label>
        `;

        switch (field.type) {
            case 'textarea':
                submissionHtml += `<textarea id="submit-${field.id}" name="submit-${field.id}" class="form-control" ${field.required ? 'required' : ''}></textarea>`;
                break;
            case 'select':
                submissionHtml += `<select id="submit-${field.id}" name="submit-${field.id}" class="form-control" ${field.required ? 'required' : ''}>
                    <option value="">Choose an option</option>`;
                field.options.forEach(option => {
                    submissionHtml += `<option value="${option}">${option}</option>`;
                });
                submissionHtml += `</select>`;
                break;
            case 'radio':
                field.options.forEach((option, index) => {
                    submissionHtml += `
                        <div style="margin-bottom: 0.5rem;">
                            <input type="radio" id="submit-${field.id}-${index}" name="submit-${field.id}" value="${option}" ${field.required ? 'required' : ''}>
                            <label for="submit-${field.id}-${index}" style="margin-left: 0.5rem;">${option}</label>
                        </div>
                    `;
                });
                break;
            case 'checkbox':
                field.options.forEach((option, index) => {
                    submissionHtml += `
                        <div style="margin-bottom: 0.5rem;">
                            <input type="checkbox" id="submit-${field.id}-${index}" name="submit-${field.id}[]" value="${option}">
                            <label for="submit-${field.id}-${index}" style="margin-left: 0.5rem;">${option}</label>
                        </div>
                    `;
                });
                break;
            case 'file':
                submissionHtml += `<input type="file" id="submit-${field.id}" name="submit-${field.id}" class="form-control" ${field.required ? 'required' : ''}>`;
                break;
            default:
                submissionHtml += `<input type="${field.type}" id="submit-${field.id}" name="submit-${field.id}" class="form-control" ${field.required ? 'required' : ''}>`;
        }

        submissionHtml += `</div>`;
    });

    // Add page navigation if multiple pages
    if (form.pages.length > 1) {
        submissionHtml += `
            <div style="display: flex; justify-content: space-between; margin-top: 2rem;">
                <button type="button" class="btn btn-secondary" ${pageIndex === 0 ? 'disabled' : ''} onclick="currentSubmissionPage--; renderSubmissionPage(getCurrentForm(), currentSubmissionPage)">Previous</button>
                <button type="button" class="btn btn-secondary" ${pageIndex === form.pages.length - 1 ? 'disabled' : ''} onclick="currentSubmissionPage++; renderSubmissionPage(getCurrentForm(), currentSubmissionPage)">Next</button>
            </div>
        `;
    }

    submissionHtml += `
            </form>
            <div class="submission-actions">
                <button class="btn btn-success" onclick="submitForm(${form.id})">Submit Form</button>
                <button class="btn btn-secondary" onclick="showPage('services')">Back to Services</button>
            </div>
        </div>
    `;

    document.getElementById('submissionContent').innerHTML = submissionHtml;
}

function getCurrentForm() {
    const formId = parseInt(document.getElementById('submissionContent').querySelector('form').id.split('-')[1]);
    return savedForms.find(f => f.id === formId);
}

function submitForm(formId) {
    const form = document.getElementById(`submissionForm-${formId}`);
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const formData = new FormData(form);
    const submissionData = {};

    for (let [key, value] of formData.entries()) {
        submissionData[key] = value;
    }

    console.log('Form submitted:', submissionData);

    const successHtml = `
        <div class="submission-success">
            <h3>🎉 Form Submitted Successfully!</h3>
            <p>Thank you for your submission. Your information has been received and will be processed shortly.</p>
        </div>
        <div style="text-align: center;">
            <button class="btn btn-primary" onclick="showPage('services')">Back to Services</button>
            <button class="btn btn-secondary" onclick="showPage('builder')">Create New Form</button>
        </div>
    `;

    document.getElementById('submissionContent').innerHTML = successHtml;
}