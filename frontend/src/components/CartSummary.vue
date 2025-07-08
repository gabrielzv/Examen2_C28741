<template>
  <div class="cart-summary">
    <h3 class="cart-title">Resumen de Compra</h3>
    
    <!-- Carrito vacio -->
    <div v-if="!hasItems" class="empty-cart">
      <p>Seleccione productos para ver el total</p>
    </div>
    
    <!-- Carrito con productos -->
    <div v-else class="cart-content">
      <!-- Items del carrito -->
      <div class="cart-items">
        <div 
          v-for="item in cartItems" 
          :key="item.productId"
          class="cart-item"
        >
          <div class="cart-item-info">
            <span class="cart-item-name">{{ getProductName(item.productId) }}</span>
            <span class="cart-item-quantity">× {{ item.quantity }}</span>
          </div>
          <div class="cart-item-price">
            {{ formatPrice(getProductPrice(item.productId) * item.quantity) }}
          </div>
        </div>
      </div>
      
      <!-- Total -->
      <div class="cart-total">
        <div class="total-label">Total:</div>
        <div class="total-amount">{{ formattedTotal }}</div>
      </div>
      
      <!-- Errores -->
      <div v-if="errors.length > 0" class="cart-errors">
        <div v-for="error in errors" :key="error" class="cart-error">
          {{ error }}
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CartSummary',
  props: {
    cartItems: {
      type: Array,
      default: () => []
    },
    products: {
      type: Array,
      default: () => []
    },
    orderSummary: {
      type: Object,
      default: null
    },
    calculating: {
      type: Boolean,
      default: false
    }
  },
  computed: {
    hasItems() {
      return this.cartItems.length > 0
    },
    formattedTotal() {
      if (this.orderSummary?.formattedTotal) {
        return this.orderSummary.formattedTotal
      }
      return '₡0'
    },
    errors() {
      return this.orderSummary?.errors || []
    }
  },
  methods: {
    getProductName(productId) {
      const product = this.products.find(p => p.id === productId)
      return product?.name || productId
    },
    getProductPrice(productId) {
      const product = this.products.find(p => p.id === productId)
      return product?.price || 0
    },
    formatPrice(amount) {
      return `₡${amount.toLocaleString()}`
    }
  }
}
</script>
