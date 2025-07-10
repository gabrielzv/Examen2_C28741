<template>
  <div class="product-card" :class="{ 'available': product.isAvailable, 'unavailable': !product.isAvailable }">
    <div class="product-header">
      <h3 class="product-name">{{ product.name }}</h3>
      <span class="product-price">{{ product.formattedPrice }}</span>
    </div>
    
    <div class="product-info">
      <div class="product-quantity">
        <span class="quantity-label">Stock:</span>
        <span 
          class="quantity-value" 
          :class="getQuantityClass(product.quantity)"
        >
          {{ product.quantity }}
        </span>
      </div>
      
      <div 
        class="status-indicator" 
        :class="{ 'status-available': product.isAvailable, 'status-unavailable': !product.isAvailable }"
      >
        {{ product.isAvailable ? 'Disponible' : 'Agotado' }}
      </div>
    </div>

    <!-- Controles -->
    <div v-if="product.isAvailable" class="product-controls">
      <div class="quantity-selector">
        <label class="quantity-selector-label">Cantidad:</label>
        <div class="quantity-input-group">
          <button 
            class="quantity-btn" 
            @click="decreaseQuantity" 
            :disabled="selectedQuantity <= 0"
          >
            -
          </button>
          <input 
            type="number" 
            class="quantity-input" 
            v-model.number="selectedQuantity"
            :min="0"
            :max="product.quantity"
            @input="validateQuantity"
          />
          <button 
            class="quantity-btn" 
            @click="increaseQuantity" 
            :disabled="selectedQuantity >= product.quantity"
          >
            +
          </button>
        </div>
      </div>
      
      <!-- Error de cantidad -->
      <div v-if="quantityError" class="quantity-error">
        {{ quantityError }}
      </div>
      
      <!-- Total -->
      <div v-if="selectedQuantity > 0" class="product-subtotal">
        Subtotal: {{ formatPrice(selectedQuantity * product.price) }}
      </div>
    </div>
  </div>
</template>

<script>
import { ref, watch } from 'vue'

export default {
  name: 'ProductCard',
  props: {
    product: {
      type: Object,
      required: true
    }
  },
  emits: ['quantity-changed'],
  setup(props, { emit }) {
    const selectedQuantity = ref(0)
    const quantityError = ref('')

    const validateQuantity = () => {
      quantityError.value = ''
      
      if (selectedQuantity.value < 0) {
        selectedQuantity.value = 0
        quantityError.value = 'La cantidad no puede ser negativa'
        return
      }
      
      if (selectedQuantity.value > props.product.quantity) {
        selectedQuantity.value = props.product.quantity
        quantityError.value = `Stock insuficiente`
        return
      }

      emit('quantity-changed', {
        productId: props.product.id,
        quantity: selectedQuantity.value
      })
    }

    const increaseQuantity = () => {
      if (selectedQuantity.value < props.product.quantity) {
        selectedQuantity.value++
        validateQuantity()
      }
    }

    const decreaseQuantity = () => {
      if (selectedQuantity.value > 0) {
        selectedQuantity.value--
        validateQuantity()
      }
    }

    const formatPrice = (amount) => {
      return `₡${amount.toLocaleString()}`
    }

    const getQuantityClass = (quantity) => {
      if (quantity === 0) return 'out-of-stock'
      if (quantity <= 3) return 'low-stock'
      return ''
    }

    watch(selectedQuantity, () => {
      validateQuantity()
    })

    return {
      selectedQuantity,
      quantityError,
      validateQuantity,
      increaseQuantity,
      decreaseQuantity,
      formatPrice,
      getQuantityClass
    }
  }
}
</script>
